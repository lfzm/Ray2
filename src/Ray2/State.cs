using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ray2
{
    /// <summary>
    /// State abstract class
    /// </summary>
    /// <typeparam name="TStateKey">State id type</typeparam>
    public abstract class State<TStateKey> : IState<TStateKey>
    {
        public State()
        {
            this.TypeCode = this.GetType().FullName;
        }
        /// <summary>
        /// State Id
        /// </summary>
        public virtual TStateKey StateId { get; set; }
        /// <summary>
        /// State version number
        /// </summary>
        public virtual long Version { get; private set; } = 0;
        /// <summary>
        /// Event time corresponding to the status version number
        /// </summary>
        public virtual long VersionTime { get; private set; }
        /// <summary>
        /// State type fullname
        /// </summary>
        public virtual string TypeCode { get; }
        /// <summary>
        /// next version no
        /// </summary>
        /// <returns></returns>
        public long NextVersion()
        {
            return this.Version++;
        }
        /// <summary>
        /// Play event modification status
        /// </summary>
        /// <param name="@event"></param>
        public void Player(IEvent @event)
        {
            this.PlayEvent(@event);
            this.Version = @event.Version;
            this.VersionTime = DateTime.Now.Ticks;
        }
        /// <summary>
        /// Play event
        /// </summary>
        /// <param name="@event">Event <see cref="IEvent"/></param>
        internal abstract void PlayEvent(IEvent @event);
    }
}
