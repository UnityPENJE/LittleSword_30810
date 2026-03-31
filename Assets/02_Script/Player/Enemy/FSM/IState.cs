namespace LittleSword.Enemy.FSM
{
    public interface IState
    {
        void Enter(Enemy enemy);
        void Update(Enemy enemy);
        void Exit(Enemy enemy);
    }
}


