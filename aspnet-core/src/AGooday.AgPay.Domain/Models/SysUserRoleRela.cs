﻿using System;
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
    /// 操作员<->角色 关联表
    /// </summary>
    [Table("t_sys_user_role_rela")]
    public class SysUserRoleRela
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required, Column("user_id", TypeName = "bigint")]
        public long UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Required, Column("role_id", TypeName = "varchar(32)")]
        public string RoleId { get; set; }
    }
}