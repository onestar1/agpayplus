﻿using AGooday.AgPay.Application.Interfaces;
using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Domain.Commands.SysUsers;
using AGooday.AgPay.Domain.Core.Bus;
using AGooday.AgPay.Domain.Interfaces;
using AGooday.AgPay.Domain.Models;
using AGooday.AgPay.Infrastructure.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGooday.AgPay.Application.Services
{
    public class MchDivisionReceiverService: IMchDivisionReceiverService
    {
        // 注意这里是要IoC依赖注入的，还没有实现
        private readonly IMchDivisionReceiverRepository _mchDivisionReceiverRepository;
        // 用来进行DTO
        private readonly IMapper _mapper;
        // 中介者 总线
        private readonly IMediatorHandler Bus;

        public MchDivisionReceiverService(IMchDivisionReceiverRepository mchDivisionReceiverRepository, IMapper mapper, IMediatorHandler bus)
        {
            _mchDivisionReceiverRepository = mchDivisionReceiverRepository;
            _mapper = mapper;
            Bus = bus;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Add(MchDivisionReceiverDto dto)
        {
            var m = _mapper.Map<MchDivisionReceiver>(dto);
            _mchDivisionReceiverRepository.Add(m);
            _mchDivisionReceiverRepository.SaveChanges();
        }

        public void Remove(long recordId)
        {
            _mchDivisionReceiverRepository.Remove(recordId);
            _mchDivisionReceiverRepository.SaveChanges();
        }

        public void Update(MchDivisionReceiverDto dto)
        {
            var m = _mapper.Map<MchDivisionReceiver>(dto);
            _mchDivisionReceiverRepository.Update(m);
            _mchDivisionReceiverRepository.SaveChanges();
        }

        public MchDivisionReceiverDto GetById(long recordId)
        {
            var entity = _mchDivisionReceiverRepository.GetById(recordId);
            var dto = _mapper.Map<MchDivisionReceiverDto>(entity);
            return dto;
        }

        public IEnumerable<MchDivisionReceiverDto> GetAll()
        {
            var mchDivisionReceivers = _mchDivisionReceiverRepository.GetAll();
            return _mapper.Map<IEnumerable<MchDivisionReceiverDto>>(mchDivisionReceivers);
        }
    }
}