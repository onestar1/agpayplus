﻿using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Common.Models;

namespace AGooday.AgPay.Application.Interfaces
{
    public interface IMchAppService : IAgPayService<MchAppDto>
    {
        Task<MchAppDto> GetByIdAsync(string recordId, string mchNo);
        Task<MchAppDto> GetByIdAsNoTrackingAsync(string recordId, string mchNo);
        IEnumerable<MchAppDto> GetByMchNoAsNoTracking(string mchNo);
        IEnumerable<MchAppDto> GetByMchNos(IEnumerable<string> mchNos);
        IEnumerable<MchAppDto> GetByAppIds(IEnumerable<string> appIds);
        Task<PaginatedList<MchAppDto>> GetPaginatedDataAsync(MchAppQueryDto dto, string agentNo = null);
    }
}
