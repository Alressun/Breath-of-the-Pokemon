using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BattleDisplay : Singleton<BattleDisplay>
{
    [OdinSerialize, Required]
    private GameObject _battleUICanvas;
    [OdinSerialize, Required]
    private GameObject _healthBar;

    private const float ChangeSelectCooldown = 0.2f;
    private float _changeSelectTimer = ChangeSelectCooldown;

    private void Awake()
    {
        if (_attackText == null || _itemText == null || _swapText == null || _runText == null)
            Debug.LogError("Text element not set.");

        Menu.Add(new MenuNode { TextDisplay = _attackText });
        Menu.Add(new MenuNode { TextDisplay = _itemText });
        Menu.Add(new MenuNode { TextDisplay = _swapText });
        Menu.Add(new MenuNode { TextDisplay = _runText });
    }

    private void Update()
    {
        if (Menu.Count != 4)
            Debug.LogError("Should be 4 menu options.");

        _changeSelectTimer += Time.deltaTime;
    }

    public void Initialize()
    {
        SelectedAction = Menu[0];

        foreach (var monster in Battle.Instance.PlayerMonsters.Union(Battle.Instance.EnemyMonsters))
        {
            var healthBar = Instantiate(_healthBar, monster.transform, false);
            healthBar.GetComponentInChildren<HealthBar>().Target = monster;
            healthBar.transform.localPosition = new Vector3(
                0, monster.GetComponent<MeshFilter>().mesh.bounds.extents.y + 0.5f, 0);
        }
    }

    public void Cleanup()
    {
        _selectCursor.transform.SetParent(_battleUICanvas.transform);
        TargetIndicator.transform.SetParent(transform);
    }

    #region Action Selection
    public List<MenuNode> Menu { get; set; } = new List<MenuNode>(4);

    [OdinSerialize, Required]
    private Text _attackText, _itemText, _swapText, _runText;
    [OdinSerialize, Required]
    private Image _selectCursor;
    [OdinSerialize, Required]
    private GameObject _moveContent, _itemContent, _monsterContent;
    [OdinSerialize, Required]
    private GameObject _actionItemPrefab;

    private MenuNode _selectedAction;
    public MenuNode SelectedAction
    {
        get { return _selectedAction; }
        set
        {
            _selectedAction = value;

            _selectCursor.transform.SetParent(SelectedAction.TextDisplay.transform, false);
            _selectCursor.transform.localPosition = new Vector3(
                -(SelectedAction.TextDisplay.rectTransform.rect.width / 2) - 15,
                0, 0);
        }
    }
    private BattleExecutable _finalizedAction;

    public const int ATTACK = 0, ITEM = 1, SWAP = 2, RUN = 3;

    public async Task<BattleExecutable> WaitForActionInput()
    {
        if (Battle.Instance.NextMove == null)
            CancelActionSelection();
        _battleUICanvas.SetActive(true);

        while (_finalizedAction == null) await Task.Delay(10);

        var tempAction = _finalizedAction;
        _finalizedAction = null;

        return tempAction;
    }

    public void ChangeActionSelection(bool moveDown)
    {
        if (_changeSelectTimer < ChangeSelectCooldown) return;

        var siblings = SelectedAction?.Parent?.Children ?? Menu;
        var selectedIndex = siblings.FindIndex(child => child == SelectedAction);

        var newIndex = selectedIndex + (moveDown ? 1 : -1);
        if (newIndex < 0)
            newIndex = siblings.Count - 1;
        if (newIndex >= siblings.Count)
            newIndex = 0;

        SelectedAction = siblings[newIndex];
        _changeSelectTimer = 0;
    }

    public void ActivateActionSelection()
    {
        var leaf = SelectedAction as LeafMenuNode;
        if (leaf != null)
        {
            _finalizedAction = leaf.Move;
        }
        else
        {
            if (SelectedAction.Children.Count == 0)
            {
                if (SelectedAction == Menu[ATTACK])
                {
                    _moveContent.transform.parent.parent.gameObject.SetActive(true);
                    AddListDisplay(Battle.Instance.CurrentMonsterTurn.Item2.Moves.ToList(), _moveContent);
                }
                else if (SelectedAction == Menu[ITEM])
                {
                    _itemContent.transform.parent.parent.gameObject.SetActive(true);
                    AddListDisplay(Battle.Instance.CurrentMonsterTurn.Item1.Items.Cast<BattleExecutable>().ToList(), _itemContent);
                }
                else if (SelectedAction == Menu[SWAP])
                {
                    _monsterContent.transform.parent.parent.gameObject.SetActive(true);
                    //TODO
                }
            }
            SelectedAction = SelectedAction.Children[0];
        }
    }

    public void CancelActionSelection()
    {
        SelectedAction = SelectedAction?.Parent ?? SelectedAction ?? Menu[0];
        SelectedAction.Children.Clear();
        
        foreach (Transform child in _moveContent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in _itemContent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in _monsterContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddListDisplay(List<BattleExecutable> executables, GameObject parentContent)
    {
        var parentRect = parentContent.GetComponent<RectTransform>();
        parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, 10);

        int i = 0;
        foreach (var executable in executables)
        {
            var contentItem = Instantiate(_actionItemPrefab);
            var height = contentItem.GetComponent<RectTransform>().rect.height;
            var text = contentItem.GetComponent<Text>();
            text.text = executable.Name;

            contentItem.transform.SetParent(parentContent.transform, false);
            contentItem.transform.localScale = Vector3.one;
            contentItem.transform.position = new Vector3(
                contentItem.transform.position.x + parentRect.rect.width / 2 + 10,
                contentItem.transform.position.y - height / 2 - (height + 3) * i - 10,
                contentItem.transform.position.z);
            parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x,
                parentRect.sizeDelta.y + height + 3);

            SelectedAction.AddChild(new LeafMenuNode() { TextDisplay = text, Move = executable });
            i++;
        }
    }
    #endregion

    #region Target Selection
    private Monster _selectedTarget;
    public Monster SelectedTarget
    {
        get { return _selectedTarget; }
        set
        {
            _selectedTarget = value;

            TargetIndicator.transform.SetParent(SelectedTarget.transform, false);
            TargetIndicator.transform.localPosition = new Vector3(
                0, SelectedTarget.GetComponent<MeshFilter>().mesh.bounds.extents.y + 1.5f, 0);
        }
    }
    private Monster _finalizedTarget;
    private bool _escapeTargetInput;

    [OdinSerialize, Required]
    private GameObject _targetIndicatorPrefab;
    private GameObject _targetIndicator;
    private GameObject TargetIndicator
    {
        get
        {
            if (_targetIndicator == null)
            {
                _targetIndicator = Instantiate(_targetIndicatorPrefab);
            }
            return _targetIndicator;
        }
        set { _targetIndicator = value; }
    }

    public async Task<Monster> WaitForTargetInput()
    {
        var targets = Battle.Instance.NextMove.ValidTargets;

        if (SelectedTarget == null || !targets.Contains(SelectedTarget))
        {
            SelectedTarget = targets.FirstOrDefault();
        }
        TargetIndicator.SetActive(true);

        while (_finalizedTarget == null)
        {
            await Task.Delay(10);
            if (_escapeTargetInput)
            {
                _escapeTargetInput = false;
                return null;
            }
        }

        _battleUICanvas.SetActive(false);
        TargetIndicator.SetActive(false);
        var tempTarget = _finalizedTarget;
        _finalizedTarget = null;

        return tempTarget;
    }

    public void ChangeTargetSelection(bool moveDown)
    {
        if (_changeSelectTimer >= ChangeSelectCooldown)
        {
            var choices = Battle.Instance.NextMove.ValidTargets;
            var selectedIndex = choices.FindIndex(monster => monster == SelectedTarget);

            var newIndex = selectedIndex + (moveDown ? 1 : -1);
            if (newIndex < 0)
                newIndex = choices.Count - 1;
            if (newIndex >= choices.Count)
                newIndex = 0;

            SelectedTarget = choices[newIndex];
            _changeSelectTimer = 0;
        }
    }

    public void ActivateTargetSelection()
    {
        _finalizedTarget = SelectedTarget;
    }

    public void CancelTargetSelection()
    {
        _escapeTargetInput = true;
        TargetIndicator.SetActive(false);
    }
    #endregion
}

public class MenuNode
{
    public Text TextDisplay { get; set; }
    public MenuNode Parent { get; set; }
    public List<MenuNode> Children { get; private set; } = new List<MenuNode>();

    public void AddChild(MenuNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public void AddChildren(List<MenuNode> children)
    {
        children.ForEach(child =>
        {
            AddChild(child);
        });
    }
}

public class LeafMenuNode : MenuNode
{
    public BattleExecutable Move { get; set; }
}
