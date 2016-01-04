﻿using System.Threading.Tasks;
using Digst.OioIdws.Rest.Common;

namespace Digst.OioIdws.Rest.AuthorizationService.Storage
{
    /// <summary>
    /// Used for storage of security tokens
    /// </summary>
    public interface ISecurityTokenStore
    {
        /// <summary>
        /// Stores the token information for the given access token
        /// </summary>
        /// <param name="accessToken">Access token being issued</param>
        /// <param name="oioIdwsToken">Token information to be stored</param>
        /// <returns></returns>
        Task StoreTokenAsync(string accessToken, OioIdwsToken oioIdwsToken);
        /// <summary>
        /// Retrieves token information for the given access token. If the token doesn't exist or has been removed due to expiration, the method should return null.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<OioIdwsToken> RetrieveTokenAsync(string accessToken);
    }
}
