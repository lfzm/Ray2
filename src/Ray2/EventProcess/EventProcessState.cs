namespace Ray2.EventHandle
{
    public class EventProcessState<TStateKey> : State<TStateKey>
    {
        internal override void PlayEvent(IEvent @event)
        {
            return;
        }
    }
}
