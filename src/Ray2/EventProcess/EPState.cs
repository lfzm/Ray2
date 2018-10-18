namespace Ray2.EventProcess
{
    public class EPState<TStateKey> : State<TStateKey>
    {
        internal override void PlayEvent(IEvent @event)
        {
            return;
        }
    }
}
