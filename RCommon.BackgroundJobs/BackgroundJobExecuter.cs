﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RCommon.BackgroundJobs
{
    public class BackgroundJobExecuter : IBackgroundJobExecuter
    {
        public ILogger<BackgroundJobExecuter> Logger { protected get; set; }

        protected BackgroundJobOptions Options { get; }

        public BackgroundJobExecuter(IOptions<BackgroundJobOptions> options)
        {
            Options = options.Value;

            Logger = NullLogger<BackgroundJobExecuter>.Instance;
        }

        public virtual async Task ExecuteAsync(JobExecutionContext context)
        {
            var job = context.ServiceProvider.GetService(context.JobType);
            if (job == null)
            {
                throw new GeneralException("The job type is not registered to DI: " + context.JobType);
            }

            var jobExecuteMethod = context.JobType.GetMethod(nameof(IBackgroundJob<object>.Execute)) ?? 
                                   context.JobType.GetMethod(nameof(IAsyncBackgroundJob<object>.ExecuteAsync));
            if (jobExecuteMethod == null)
            {
                throw new GeneralException($"Given job type does not implement {typeof(IBackgroundJob<>).Name} or {typeof(IAsyncBackgroundJob<>).Name}. " +
                                       "The job type was: " + context.JobType);
            }

            try
            {
                if (jobExecuteMethod.Name == nameof(IAsyncBackgroundJob<object>.ExecuteAsync))
                {
                    await ((Task) jobExecuteMethod.Invoke(job, new[] {context.JobArgs}));
                }
                else
                {
                    jobExecuteMethod.Invoke(job, new[] { context.JobArgs });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);

                await context.ServiceProvider
                    .GetRequiredService<IExceptionNotifier>()
                    .NotifyAsync(new ExceptionNotificationContext(ex));

                throw new BackgroundJobExecutionException("A background job execution is failed. See inner exception for details.", ex)
                {
                    JobType = context.JobType.AssemblyQualifiedName,
                    JobArgs = context.JobArgs
                };
            }
        }
    }
}