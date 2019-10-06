using UnityEngine;

public class ControlManager : MonoBehaviour
{
    public float Speed;
    public float DistanceThreshold;
    public Transform Bazaar;

    Transform _transform;
    Animator _animator;
    SpriteRenderer _spriteRenderer;

    Transform _current;

    bool _menu;

    public int Score { get; private set; }
    public string Item { get; private set; }

    public AudioSource Open;
    public AudioSource Close;

    void Start()
    {
        _transform = transform;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _current = null;
        _menu = false;

        Score = 0;
        Item = "Nothing";
    }

    void Update()
    {
        if (_menu)
            return;

        float input = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;

        if (Mathf.Abs(input) == 1f)
        {
            _spriteRenderer.flipX = input < 0f;

            float offset = Speed * Time.deltaTime * input;

            if (Mathf.Abs(_transform.position.x + offset) < 4.5f)
            {
                _transform.Translate(offset * Vector3.right);
            }

            if (!_animator.GetBool("Walk"))
            {
                _animator.SetBool("Walk", true);
            }
        }
        else if (Mathf.Abs(input) != 1f && _animator.GetBool("Walk"))
        {
            _animator.SetBool("Walk", false);
        }

        float nearestDistance = float.MaxValue;
        Transform nearestStore = null;
        for (int i=0; i<Bazaar.childCount; i++)
        {
            float distance = Vector3.Distance(_transform.position, Bazaar.GetChild(i).position);
            if (nearestDistance > distance && distance < DistanceThreshold)
            {
                nearestDistance = distance;
                nearestStore = Bazaar.GetChild(i);
            }
        }


        if (nearestStore != _current)
        {

            if (_current != null)
            {
                Close.Play();
                _current.GetComponent<NPC>().Unselect();
            }
            _current = nearestStore;
            if (_current != null)
            {
                Open.Play();
                _current.GetComponent<NPC>().Select();
            }
        }

        if (_current && Input.GetKeyDown(KeyCode.W) && !_menu)
        {
            Bazaar.GetComponent<AudioSource>().Play();
            _menu = true;
            Transform temp = _current;
            _current.GetComponent<NPC>().Unselect();
            _current = temp;
            _current.GetComponent<NPC>().ToggleMainMenu(true);
        }
    }

    public void CloseMenu()
    {
        _current.GetComponent<NPC>().ToggleMainMenu(false);
        Bazaar.GetComponent<AudioSource>().Play();
        _current = null;
    }

    public void Closed()
    {
        _menu = false;
    }

    public void Give(string i, int c)
    {
        GetComponent<AudioSource>().Play();
        Item = i;
        Score = c;
    }

    public bool InMenu() { return _menu; }

    public void ForceUnselect()
    {
        _current?.GetComponent<NPC>().ForceUnselect();
    }
}
