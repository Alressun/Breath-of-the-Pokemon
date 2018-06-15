using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Trainer : SerializedMonoBehaviour
{
    [OdinSerialize]
    public int Level { get; set; } = 1;

    [OdinSerialize] private Team _team = new Team();
    public Team Team
    {
        get { return _team; }
        set
        {
            _team.Cleanup();
            _team = value;
            _team.Init();
        }
    }

    [OdinSerialize]
    public List<Item> Items { get; set; } = new List<Item>();

    private void Awake()
    {
        Team.Init();
    }

    public async Task<BattleExecutable> SelectMove(Monster monster)
    {
        if (CompareTag(Tags.Player))
            return await BattleDisplay.Instance.WaitForActionInput();
        return monster.SelectMove();
    }

    public async Task<Monster> SelectTarget(Monster monster)
    {
        if (CompareTag(Tags.Player))
            return await BattleDisplay.Instance.WaitForTargetInput();
        return monster.SelectTarget();
    }
}