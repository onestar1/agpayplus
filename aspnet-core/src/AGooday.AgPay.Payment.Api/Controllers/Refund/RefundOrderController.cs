﻿using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Application.Interfaces;
using AGooday.AgPay.Application.Services;
using AGooday.AgPay.Common.Enumerator;
using AGooday.AgPay.Common.Exceptions;
using AGooday.AgPay.Common.Models;
using AGooday.AgPay.Common.Utils;
using AGooday.AgPay.Payment.Api.Channel;
using AGooday.AgPay.Payment.Api.Exceptions;
using AGooday.AgPay.Payment.Api.Models;
using AGooday.AgPay.Payment.Api.RQRS.Msg;
using AGooday.AgPay.Payment.Api.RQRS.Refund;
using AGooday.AgPay.Payment.Api.Services;
using AGooday.AgPay.Payment.Api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AGooday.AgPay.Payment.Api.Controllers.Refund
{
    [ApiController]
    public class RefundOrderController : ApiControllerBase
    {
        protected readonly Func<string, IRefundService> _refundServiceFactory;
        protected readonly ILogger<RefundOrderController> _logger;
        private readonly IPayOrderService _payOrderService;
        private readonly IRefundOrderService _refundOrderService;
        private readonly PayMchNotifyService _payMchNotifyService;
        private readonly ConfigContextQueryService _configContextQueryService;

        public RefundOrderController(Func<string, IRefundService> refundServiceFactory,
            ILogger<RefundOrderController> logger,
            IPayOrderService payOrderService,
            IRefundOrderService refundOrderService,
            PayMchNotifyService payMchNotifyService,
            ConfigContextQueryService configContextQueryService,
            RequestIpUtil requestIpUtil)
            : base(requestIpUtil)
        {
            _refundServiceFactory = refundServiceFactory;
            _logger = logger;
            _payOrderService = payOrderService;
            _refundOrderService = refundOrderService;
            _payMchNotifyService = payMchNotifyService;
            _configContextQueryService = configContextQueryService;
        }

        [HttpPost, Route("api/refund/refundOrder")]
        public ApiRes RefundOrder(RefundOrderRQ rq)
        {
            RefundOrderDto refundOrder = null;

            try
            {
                if (string.IsNullOrWhiteSpace(rq.MchOrderNo) && string.IsNullOrWhiteSpace(rq.PayOrderId))
                {
                    throw new BizException("mchOrderNo 和 payOrderId不能同时为空");
                }

                PayOrderDto payOrder = _payOrderService.QueryMchOrder(rq.MchNo, rq.PayOrderId, rq.MchOrderNo);
                if (payOrder == null)
                {
                    throw new BizException("退款订单不存在");
                }

                if (payOrder.State != (byte)PayOrderState.STATE_SUCCESS)
                {
                    throw new BizException("订单状态不正确， 无法完成退款");
                }

                if (payOrder.RefundState == (byte)PayOrderRefund.REFUND_STATE_ALL || payOrder.RefundAmount >= payOrder.Amount)
                {
                    throw new BizException("订单已全额退款，本次申请失败");
                }

                if (payOrder.RefundAmount + rq.RefundAmount > payOrder.Amount)
                {
                    throw new BizException("申请金额超出订单可退款余额，请检查退款金额");
                }

                if (_refundOrderService.IsExistRefundingOrder(payOrder.PayOrderId))
                {
                    throw new BizException("支付订单具有在途退款申请，请稍后再试");
                }

                //全部退款金额 （退款订单表）
                long sumSuccessRefundAmount = _refundOrderService.SumSuccessRefundAmount(payOrder.PayOrderId);
                if (sumSuccessRefundAmount >= payOrder.Amount)
                {
                    throw new BizException("退款单已完成全部订单退款，本次申请失败");
                }

                if (sumSuccessRefundAmount + rq.RefundAmount > payOrder.Amount)
                {
                    throw new BizException("申请金额超出订单可退款余额，请检查退款金额");
                }

                string mchNo = rq.MchNo;
                string appId = rq.AppId;

                // 校验退款单号是否重复
                if (_refundOrderService.IsExistOrderByMchOrderNo(mchNo, rq.MchRefundNo))
                {
                    throw new BizException("商户退款订单号[" + rq.MchRefundNo + "]已存在");
                }

                if (!string.IsNullOrWhiteSpace(rq.NotifyUrl) && !StringUtil.IsAvailableUrl(rq.NotifyUrl))
                {
                    throw new BizException("异步通知地址协议仅支持http:// 或 https:// !");
                }

                //获取支付参数 (缓存数据) 和 商户信息
                MchAppConfigContext mchAppConfigContext = _configContextQueryService.QueryMchInfoAndAppInfo(mchNo, appId);
                if (mchAppConfigContext == null)
                {
                    throw new BizException("获取商户应用信息失败");
                }

                MchInfoDto mchInfo = mchAppConfigContext.MchInfo;
                MchAppDto mchApp = mchAppConfigContext.MchApp;

                //获取退款接口
                IRefundService refundService = _refundServiceFactory(payOrder.IfCode);
                if (refundService == null)
                {
                    throw new BizException("当前通道不支持退款！");
                }

                refundOrder = GenRefundOrder(rq, payOrder, mchInfo, mchApp);

                //退款单入库 退款单状态：生成状态  此时没有和任何上游渠道产生交互。
                _refundOrderService.Add(refundOrder);

                // 调起退款接口
                ChannelRetMsg channelRetMsg = refundService.Refund(rq, refundOrder, payOrder, mchAppConfigContext);

                //处理退款单状态
                this.ProcessChannelMsg(channelRetMsg, refundOrder);

                RefundOrderRS bizRes = RefundOrderRS.BuildByRefundOrder(refundOrder);
                return ApiRes.OkWithSign(bizRes, _configContextQueryService.QueryMchApp(rq.MchNo, rq.AppId).AppSecret);
            }
            catch (BizException e)
            {
                return ApiRes.CustomFail(e.Message);
            }
            catch (ChannelException e)
            {
                //处理上游返回数据
                this.ProcessChannelMsg(e.ChannelRetMsg, refundOrder);

                if (e.ChannelRetMsg.ChannelState == ChannelState.SYS_ERROR)
                {
                    return ApiRes.CustomFail(e.Message);
                }

                RefundOrderRS bizRes = RefundOrderRS.BuildByRefundOrder(refundOrder);
                return ApiRes.OkWithSign(bizRes, _configContextQueryService.QueryMchApp(rq.MchNo, rq.AppId).AppSecret);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"系统异常：{e.Message}");
                return ApiRes.CustomFail("系统异常");
            }
        }

        private RefundOrderDto GenRefundOrder(RefundOrderRQ rq, PayOrderDto payOrder, MchInfoDto mchInfo, MchAppDto mchApp)
        {
            DateTime nowTime = DateTime.Now;
            RefundOrderDto refundOrder = new RefundOrderDto();
            refundOrder.RefundOrderId = SeqUtil.GenRefundOrderId(); //退款订单号
            refundOrder.PayOrderId = payOrder.PayOrderId; //支付订单号
            refundOrder.ChannelPayOrderNo = payOrder.ChannelOrderNo; //渠道支付单号
            refundOrder.MchNo = mchInfo.MchNo; //商户号
            refundOrder.IsvNo = mchInfo.IsvNo; //服务商号
            refundOrder.AppId = mchApp.AppId; //商户应用ID
            refundOrder.MchName = mchInfo.MchShortName; //商户名称
            refundOrder.MchType = mchInfo.Type; //商户类型
            refundOrder.MchRefundNo = rq.MchRefundNo; //商户退款单号
            refundOrder.WayCode = payOrder.WayCode; //支付方式代码
            refundOrder.IfCode = payOrder.IfCode; //支付接口代码
            refundOrder.PayAmount = payOrder.Amount; //支付金额,单位分
            refundOrder.RefundAmount = rq.RefundAmount; //退款金额,单位分
            refundOrder.Currency = rq.Currency; //三位货币代码,人民币:cny
            refundOrder.State = (byte)RefundOrderState.STATE_INIT; //退款状态:0-订单生成,1-退款中,2-退款成功,3-退款失败
            refundOrder.ClientIp = string.IsNullOrWhiteSpace(rq.ClientIp) ? GetClientIp() : rq.ClientIp; //客户端IP
            refundOrder.RefundReason = rq.RefundReason; //退款原因
            refundOrder.ChannelOrderNo = null; //渠道订单号
            refundOrder.ErrCode = null; //渠道错误码
            refundOrder.ErrMsg = null; //渠道错误描述
            refundOrder.ChannelExtra = rq.ChannelExtra; //特定渠道发起时额外参数
            refundOrder.NotifyUrl = rq.NotifyUrl; //通知地址
            refundOrder.ExtParam = rq.ExtParam; //扩展参数
            refundOrder.ExpiredTime = nowTime.AddHours(2); //订单超时关闭时间 默认两个小时
            refundOrder.SuccessTime = null; //订单退款成功时间
            refundOrder.CreatedAt = nowTime; //创建时间

            return refundOrder;
        }

        /// <summary>
        /// 处理返回的渠道信息，并更新退款单状态
        /// payOrder将对部分信息进行 赋值操作。
        /// </summary>
        /// <param name="channelRetMsg"></param>
        /// <param name="refundOrder"></param>
        /// <exception cref="BizException"></exception>
        private void ProcessChannelMsg(ChannelRetMsg channelRetMsg, RefundOrderDto refundOrder)
        {
            //对象为空 || 上游返回状态为空， 则无需操作
            if (channelRetMsg == null || channelRetMsg.ChannelState == null)
            {
                return;
            }

            //明确成功
            if (ChannelState.CONFIRM_SUCCESS == channelRetMsg.ChannelState)
            {
                this.UpdateInitOrderStateThrowException((byte)RefundOrderState.STATE_SUCCESS, refundOrder, channelRetMsg);
                _payMchNotifyService.RefundOrderNotify(refundOrder);
            }
            //明确失败
            else if (ChannelState.CONFIRM_FAIL == channelRetMsg.ChannelState)
            {
                this.UpdateInitOrderStateThrowException((byte)RefundOrderState.STATE_FAIL, refundOrder, channelRetMsg);
                _payMchNotifyService.RefundOrderNotify(refundOrder);
            }
            // 上游处理中 || 未知 || 上游接口返回异常  退款单为退款中状态
            else if (ChannelState.WAITING == channelRetMsg.ChannelState ||
                ChannelState.UNKNOWN == channelRetMsg.ChannelState ||
                ChannelState.API_RET_ERROR == channelRetMsg.ChannelState)
            {
                this.UpdateInitOrderStateThrowException((byte)RefundOrderState.STATE_ING, refundOrder, channelRetMsg);
            }
            // 系统异常：  退款单不再处理。  为： 生成状态
            else if (ChannelState.SYS_ERROR == channelRetMsg.ChannelState)
            {
            }
            else
            {
                throw new BizException("ChannelState 返回异常！");
            }
        }

        /// <summary>
        /// 更新退款单状态 --》 退款单生成--》 其他状态  (向外抛出异常) 
        /// </summary>
        /// <param name="orderState"></param>
        /// <param name="refundOrder"></param>
        /// <param name="channelRetMsg"></param>
        /// <exception cref="BizException"></exception>
        private void UpdateInitOrderStateThrowException(byte orderState, RefundOrderDto refundOrder, ChannelRetMsg channelRetMsg)
        {
            refundOrder.State = orderState;
            refundOrder.ChannelOrderNo = channelRetMsg.ChannelOrderId;
            refundOrder.ErrCode = channelRetMsg.ChannelErrCode;
            refundOrder.ErrMsg = channelRetMsg.ChannelErrMsg;

            bool isSuccess = _refundOrderService.UpdateInit2Ing(refundOrder.RefundOrderId);
            if (!isSuccess)
            {
                throw new BizException("更新退款单异常!");
            }

            isSuccess = _refundOrderService.UpdateIng2SuccessOrFail(refundOrder.RefundOrderId, refundOrder.State,
                channelRetMsg.ChannelOrderId, channelRetMsg.ChannelErrCode, channelRetMsg.ChannelErrMsg);
            if (!isSuccess)
            {
                throw new BizException("更新退款单异常!");
            }
        }
    }
}