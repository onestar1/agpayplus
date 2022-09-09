﻿using AGooday.AgPay.Domain.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGooday.AgPay.Domain.Models
{
    /// <summary>
    /// 支付订单表
    /// </summary>
    [Table("t_pay_order")]
    public class PayOrder
    {
        /// <summary>
        /// 支付订单号
        /// </summary>
        [Key, Required, Column("pay_order_id", TypeName = "varchar(30)")]
        public string PayOrderId { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [Required, Column("mch_no", TypeName = "varchar(64)")]
        public string MchNo { get; set; }

        /// <summary>
        /// 服务商号
        /// </summary>
        [Column("isv_no", TypeName = "varchar(64)")]
        public string IsvNo { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        [Required, Column("app_id", TypeName = "varchar(64)")]
        public string AppId { get; set; }

        /// <summary>
        /// 商户名称
        /// </summary>
        [Required, Column("mch_name", TypeName = "varchar(30)")]
        public string MchName { get; set; }

        /// <summary>
        /// 类型: 1-普通商户, 2-特约商户(服务商模式)
        /// </summary>
        [Required, Column("mch_type", TypeName = "tinyint")]
        public byte MchType { get; set; }

        /// <summary>
        /// 商户订单号
        /// </summary>
        [Required, Column("mch_order_no", TypeName = "varchar(64)")]
        public string MchOrderNo { get; set; }

        /// <summary>
        /// 支付接口代码
        /// </summary>
        [Column("if_code", TypeName = "varchar(20)")]
        public string IfCode { get; set; }

        /// <summary>
        /// 支付方式代码
        /// </summary>
        [Required, Column("way_code", TypeName = "varchar(20)")]
        public string WayCode { get; set; }

        /// <summary>
        /// 支付金额,单位分
        /// </summary>
        [Required, Column("amount", TypeName = "bigint")]
        public long Amount { get; set; }

        /// <summary>
        /// 商户手续费费率快照
        /// </summary>
        [Required, Column("mch_fee_rate", TypeName = "decimal(20,6)")]
        public decimal MchFeeRate { get; set; }

        /// <summary>
        /// 商户手续费,单位分
        /// </summary>
        [Required, Column("mch_fee_amount", TypeName = "bigint")]
        public long MchFeeAmount { get; set; }

        /// <summary>
        /// 三位货币代码,人民币:cny
        /// </summary>
        [Required, Column("currency", TypeName = "varchar(3)")]
        public string Currency { get; set; }

        /// <summary>
        /// 支付状态: 0-订单生成, 1-支付中, 2-支付成功, 3-支付失败, 4-已撤销, 5-已退款, 6-订单关闭
        /// </summary>
        [Required, Column("state", TypeName = "tinyint")]
        public byte State { get; set; }

        /// <summary>
        /// 向下游回调状态, 0-未发送,  1-已发送
        /// </summary>
        [Required, Column("notify_state", TypeName = "tinyint")]
        public byte NotifyState { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        [Column("client_ip", TypeName = "varchar(32)")]
        public string ClientIp { get; set; }

        /// <summary>
        /// 商品标题
        /// </summary>
        [Required, Column("subject", TypeName = "varchar(64)")]
        public string Subject { get; set; }

        /// <summary>
        /// 商品描述信息
        /// </summary>
        [Required, Column("body", TypeName = "varchar(256)")]
        public string Body { get; set; }

        /// <summary>
        /// 特定渠道发起额外参数
        /// </summary>
        [Column("channel_extra", TypeName = "varchar(512)")]
        public string ChannelExtra { get; set; }

        /// <summary>
        /// 渠道用户标识,如微信openId,支付宝账号
        /// </summary>
        [Column("channel_user", TypeName = "varchar(64)")]
        public string ChannelUser { get; set; }

        /// <summary>
        /// 渠道订单号
        /// </summary>
        [Column("channel_order_no", TypeName = "varchar(64)")]
        public string ChannelOrderNo { get; set; }

        /// <summary>
        /// 退款状态: 0-未发生实际退款, 1-部分退款, 2-全额退款
        /// </summary>
        [Required, Column("refund_state", TypeName = "tinyint")]
        public byte RefundState { get; set; }

        /// <summary>
        /// 退款次数
        /// </summary>
        [Required, Column("refund_times", TypeName = "int")]
        public int RefundTimes { get; set; }

        /// <summary>
        /// 退款总金额,单位分
        /// </summary>
        [Required, Column("refund_amount", TypeName = "bigint")]
        public long RefundAmount { get; set; }

        /// <summary>
        /// 订单分账模式：0-该笔订单不允许分账, 1-支付成功按配置自动完成分账, 2-商户手动分账(解冻商户金额)
        /// </summary>
        [Column("division_mode", TypeName = "tinyint")]
        public byte DivisionMode { get; set; }

        /// <summary>
        /// 0-未发生分账, 1-等待分账任务处理, 2-分账处理中, 3-分账任务已结束(不体现状态)
        /// </summary>
        [Column("division_state", TypeName = "tinyint")]
        public byte DivisionState { get; set; }

        /// <summary>
        /// 最新分账时间
        /// </summary>
        [Column("division_last_time", TypeName = "datetime")]
        public DateTime DivisionLastTime { get; set; }

        /// <summary>
        /// 渠道支付错误码
        /// </summary>
        [Column("err_code", TypeName = "varchar(128)")]
        public string ErrCode { get; set; }

        /// <summary>
        /// 渠道支付错误描述
        /// </summary>
        [Column("err_msg", TypeName = "varchar(256)")]
        public string ErrMsg { get; set; }

        /// <summary>
        /// 商户扩展参数
        /// </summary>
        [Column("ext_param", TypeName = "varchar(128)")]
        public string ExtParam { get; set; }

        /// <summary>
        /// 异步通知地址
        /// </summary>
        [Required, Column("notify_url", TypeName = "varchar(128)")]
        public string NotifyUrl { get; set; }

        /// <summary>
        /// 页面跳转地址
        /// </summary>
        [Column("return_url", TypeName = "varchar(128)")]
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 订单失效时间
        /// </summary>
        [Column("expired_time", TypeName = "datetime")]
        public DateTime ExpiredTime { get; set; }

        /// <summary>
        /// 订单支付成功时间
        /// </summary>
        [Column("success_time", TypeName = "datetime")]
        public DateTime SuccessTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required, Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Required, Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}