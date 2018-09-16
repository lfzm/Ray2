using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    /// <summary>
    /// This is the state interface 
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// State version number
        /// </summary>
        Int64 Version { get; }
        /// <summary>
        /// Event time corresponding to the status version number
        /// </summary>
        long VersionTime { get; }
        /// <summary>
        /// State type fullname
        /// </summary>
        string TypeCode { get; }
        /// <summary>
        /// Play event modification status
        /// </summary>
        /// <param name="evt"></param>
        void Player(IEvent @event);
        /// <summary>
        /// next version no
        /// </summary>
        /// <returns></returns>
        Int64 NextVersion();
    }
    /// <summary>
    /// This is the state interface 
    /// </summary>
    public interface IState<TStateKey> : IState
    {
        /// <summary>
        /// State Id
        /// </summary>
        TStateKey StateId { get; set; }
    }
}
