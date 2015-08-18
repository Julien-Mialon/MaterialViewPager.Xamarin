namespace KenBurnsView
{
	public interface ITransitionListener
	{
		/**
		 * Notifies the start of a transition.
		 * @param transition the transition that just started.
		 */
		void OnTransitionStart(Transition transition);

		/**
		 * Notifies the end of a transition.
		 * @param transition the transition that just ended.
		 */
		void OnTransitionEnd(Transition transition);
	}
}