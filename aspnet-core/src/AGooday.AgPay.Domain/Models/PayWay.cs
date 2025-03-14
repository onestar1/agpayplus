﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AGooday.AgPay.Domain.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AGooday.AgPay.Domain.Models
{
    /// <summary>
    /// 支付方式表
    /// </summary>
    [Comment("支付方式表")]
    [Table("t_pay_way")]
    public class PayWay : AbstractTrackableTimestamps
    {
        /// <summary>
        /// 支付方式代码  例如： wxpay_jsapi
        /// </summary>
        [Comment("支付方式代码  例如： wxpay_jsapi")]
        [Key, Required, Column("way_code", TypeName = "varchar(20)")]
        public string WayCode { get; set; }

        /// <summary>
        /// 支付方式名称
        /// </summary>
        [Comment("支付方式名称")]
        [Required, Column("way_name", TypeName = "varchar(20)")]
        public string WayName { get; set; }

        /// <summary>
        /// 支付类型: WECHAT-微信, ALIPAY-支付宝, YSFPAY-云闪付, UNIONPAY-银联, DCEPPAY-数字人民币, OTHER-其他
        /// </summary>
        [Comment("支付类型: WECHAT-微信, ALIPAY-支付宝, YSFPAY-云闪付, UNIONPAY-银联, DCEPPAY-数字人民币, OTHER-其他")]
        [Required, Column("way_type", TypeName = "varchar(20)")]
        public string WayType { get; set; }
    }
}
