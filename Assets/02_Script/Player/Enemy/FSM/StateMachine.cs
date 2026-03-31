namespace LittleSword.Enemy.FSM
{
    public class StateMachine
    {
        private Enemy enemy;
        public StateMachine(Enemy enemy)
        {
            this.enemy = enemy;
        }

        public IState currentState { get; private set; }

        public void ChangeState(IState newState)
        {
            currentState?.Exit(enemy);
            currentState = newState;
            currentState.Enter(enemy);
        }



        // Update is called once per frame
        public void Update()
        {
            currentState?.Update(enemy);
        }
    }
}

