using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

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
      [JsonProperty]
        public virtual long Version { get; private set; } = 0;
        /// <summary>
        /// Event time corresponding to the status version number
        /// </summary>
        [JsonProperty]
        public virtual long VersionTime { get; private set; }
        /// <summary>
        /// State type fullname
        /// </summary>
        [JsonProperty]
        public virtual string TypeCode { get; }
        /// <summary>
        /// next version no
        /// </summary>
        /// <returns></returns>
        public long NextVersion()
        {
            return this.Version+1;
        }
        /// <summary>
        /// Play event modification status
        /// </summary>
        /// <param name="@event"></param>
        public void Player(IEvent @event)
        {
            if (@event == null)
                return;
            this.PlayEvent(@event);
            this.Version = @event.Version;
            this.VersionTime = @event.Timestamp;
        }
        public void Player(IList<IEvent> events)
        {
            if (events == null || events.Count == 0)
                return ;
            foreach (var @event in events)
            {
                this.Player( @event);
            }
        }
        public void Player(IList<IEvent<TStateKey>> events)
        {
            if (events == null || events.Count == 0)
                return;
            foreach (var @event in events)
            {
                this.Player(@event);
            }
        }

        /// <summary>
        /// Play event
        /// </summary>
        /// <param name="@event">Event <see cref="IEvent"/></param>
        protected abstract void PlayEvent(IEvent @event);
    }
}
