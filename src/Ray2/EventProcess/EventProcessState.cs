namespace Ray2.EventProcess
{
    public class EventProcessState<TStateKey> : State<TStateKey>
    {
        internal override void PlayEvent(IEvent @event)
        {
            return;
        }
    }
}
