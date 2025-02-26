﻿namespace MoneyFox.Infrastructure.DbBackup
{

    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    internal interface IOneDriveAuthenticationService
    {
        Task<GraphServiceClient> CreateServiceClient(CancellationToken cancellationToken = default);

        Task LogoutAsync(CancellationToken cancellationToken = default);
    }

}
