﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGooday.AgPay.Application.DataTransfer
{
    /// <summary>
    /// 系统用户表
    /// </summary>
    public class SysUserCreateDto
    {
        /// <summary>
        /// 登录用户名
        /// </summary>
        public string LoginUsername { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string Realname { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Telphone { get; set; }

        /// <summary>
        /// 性别 0-未知, 1-男, 2-女
        /// </summary>
        public byte Sex { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        [BindNever]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// 员工编号
        /// </summary>
        public string UserNo { get; set; }

        /// <summary>
        /// 是否超管（超管拥有全部权限） 0-否 1-是
        /// </summary>
        public byte IsAdmin { get; set; }

        /// <summary>
        /// 状态 0-停用 1-启用
        /// </summary>
        public byte State { get; set; }

        /// <summary>
        /// 所属系统： MGR-运营平台, MCH-商户中心
        /// </summary>
        [BindNever]
        public string SysType { get; set; }

        /// <summary>
        /// 所属商户ID / 0(平台)
        /// </summary>
        [BindNever]
        public string BelongInfoId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [BindNever]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [BindNever]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}