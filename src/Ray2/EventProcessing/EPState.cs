namespace Ray2.EventProcessing
{
    public class EPState<TStateKey> : State<TStateKey>
    {
        internal override void PlayEvent(IEvent @event)
        {
            return;
        }
    }
}
