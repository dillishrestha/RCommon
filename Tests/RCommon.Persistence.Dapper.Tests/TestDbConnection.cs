﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RCommon.BusinessEntities;
using RCommon.DataServices.Sql;
using RCommon.Mediator;
using System;
using System.Collections.Generic;
using System.Text;

namespace RCommon.Persistence.Dapper.Tests
{
    public class TestDbConnection : RDbConnection
    {

        public TestDbConnection(IOptions<RDbConnectionOptions> options, IEventTracker eventTracker, IMediatorService mediator) 
            : base(options, eventTracker, mediator)
        {
            
        }
    }
}
