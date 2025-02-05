using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Character3DWorld : MonoBehaviour
{
    [Header("╔══════════════════════╗")]
    [Space(-13)]
    [Header("║              ２-D CHARACTER            ║")]
    [Space(-13)]
    [Header("║                IN A ３-D WORLD             ║")]
    [Space(-13)]
    [Header("║                                                               ║")]
    [Space(-13)]
    [Header("║         Made by Bash&ChatGpt        ║")]
    [Space(-13)]
    [Header("╚══════════════════════╝")]
    [Space(10)]

    [Tooltip("Drag the Main Camera object here")]
    public Transform TheCamera;
    [Tooltip("Drag the object that has the Animator and Sprite Renderer components here")]
    public GameObject billboard;

    [Header("Instructions:")]
    [Space(-13)]
    [Header("┌─┬─┬─┐  0 is the front of '웃'")]
    [Space(-19)]
    [Header("  ７  ０  １")]
    [Space(-19)]
    [Header("├─┼─┼─┤  '웃' is this gameObject")]
    [Space(-19)]
    [Header("  ６  웃  ２")]
    [Space(-19)]
    [Header("├─┼─┼─┤  the order of the sections")]
    [Space(-19)]
    [Header("  ５  ４  ３")]
    [Space(-19)]
    [Header("└─┴─┴─┘  is clockwise ↻ from 0 to 7")]
    [Space(-13)]
    [Header("웃 change depending where # │̲̲̅̅⦿̲̅°̲̅l̲̅ is")]

    [Header("Rendering: Animator/Sprites")]
    [Tooltip("Enable or Disable the animator to change between sprite rendering to animations")]
    public bool useAnimator = false;
    [Tooltip("if enabled you can render a character with only 4 directions instead of 8")]
    public bool use4Directions = false;

    public List<Sprite> sprites = new List<Sprite>(8);
    [HideInInspector]
    public List<Sprite> HiddenSprites8 = new List<Sprite>(8);
    [HideInInspector]
    public List<Sprite> HiddenSprites4 = new List<Sprite>(4);

    [Header("Character: Simetric/Asimetric")]
    [Tooltip("the 2,3,4 sprites/animations will be flipX and shown when the 6,7,8 are called")]
    public bool useMirror;

    enum Facing { Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft };
    private Facing _facing;
    private float angle;
    private float sign = 1;
    private float signAngle;
    private Animator animator;
    private SpriteRenderer _sprite;
    private Transform _billboard;
    private Transform _t;
    private Vector3 direction;
    private bool tOutcome;

    void OnValidate()
    {
        if (billboard == null)
        {
            Debug.Log("you need to assing the object billboard that has the Animator and Sprite Renderer components to use this script");
        }
        else
        {
            animator = billboard.GetComponent<Animator>();
        }

        if (use4Directions == true)
        {
            if (sprites.Count == 4)
            {
                HiddenSprites4.Clear();
                HiddenSprites4.InsertRange(0, sprites);
            }
            else if (sprites.Count != 4)
            {
                sprites.Clear();
                sprites.InsertRange(0, HiddenSprites4);
            }
        }
        if (use4Directions == false)
        {
            if (sprites.Count == 8)
            {
                HiddenSprites8.Clear();
                HiddenSprites8.InsertRange(0, sprites);
            }
            else if (sprites.Count != 8)
            {
                sprites.Clear();
                sprites.InsertRange(0, HiddenSprites8);
            }
        }

        if (useAnimator)
        {
            if (animator != null)
            {
                animator.enabled = false;
                animator.enabled = true;
            }

            if (animator != null && animator.isActiveAndEnabled)
            {
                tOutcome = ContainsParam("direction");
                if (tOutcome == false)
                {
                    Debug.Log("you need to create a INT parameter called 'direction' in the animatorController of the billdoard object in order to work with the script '2D character In a 3D world' animation feature");
                }
            }
        }
        else
        {
            animator.enabled = false;
        }
    }

    public bool ContainsParam(string _ParamName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == _ParamName) return true;
        }
        return false;
    }

    void Awake()
    {
        _t = transform;
        if (billboard != null)
        {
            animator = billboard.GetComponent<Animator>();
            _sprite = billboard.GetComponent<SpriteRenderer>();
            _billboard = billboard.transform;
        }
    }

    void Update()
    {
        Vector3 forward = _t.forward;
        forward.y = 0;
        Vector3 direction2 = _t.InverseTransformPoint(TheCamera.position);
        sign = (direction2.x >= 0) ? -1 : 1;
        angle = Vector3.Angle(direction, forward);
        signAngle = angle * sign;
        direction = TheCamera.position - _t.position;
        direction.y = 0;
        _billboard.rotation = Quaternion.LookRotation(-direction, _t.up);

        if (useMirror)
        {
            Miror();
        }
        else
        {
            _sprite.flipX = false;
        }

        if (use4Directions == false)
        {
            if (animator.isActiveAndEnabled == true)
            {
                animator.SetInteger("direction", (int)_facing);
            }
            else
            {
                _sprite.sprite = sprites[(int)_facing];
            }
        }
        else if (use4Directions == true)
        {
            if (animator.isActiveAndEnabled == true)
            {
                animator.SetInteger("direction", (int)_facing);
            }
            else
            {
                _sprite.sprite = sprites[(int)_facing / 2];
            }
        }
    }

    public virtual void LateUpdate()
    {
        if (use4Directions == false)
        {
            if (angle < 22.5f) _facing = Facing.Up;
            else if (angle < 67.5f) _facing = sign < 0 ? Facing.UpRight : Facing.UpLeft;
            else if (angle < 112.5f) _facing = sign < 0 ? Facing.Right : Facing.Left;
            else if (angle < 157.5f) _facing = sign < 0 ? Facing.DownRight : Facing.DownLeft;
            else _facing = Facing.Down;
        }
        else if (use4Directions == true)
        {
            if (angle < 45.0f) _facing = Facing.Up;
            else if (angle < 135.0f) _facing = sign < 0 ? Facing.Right : Facing.Left;
            else _facing = Facing.Down;
        }
    }

    public void Miror()
    {
        if (use4Directions == false)
        {
            switch (_facing)
            {
                case Facing.DownLeft:
                    _facing = Facing.DownRight;
                    _sprite.flipX = true;
                    break;
                case Facing.Left:
                    _facing = Facing.Right;
                    _sprite.flipX = true;
                    break;
                case Facing.UpLeft:
                    _facing = Facing.UpRight;
                    _sprite.flipX = true;
                    break;
                default:
                    _sprite.flipX = false;
                    break;
            }
        }
        if (use4Directions == true)
        {
            switch (_facing)
            {
                case Facing.Left:
                    _facing = Facing.Right;
                    _sprite.flipX = true;
                    break;
                default:
                    _sprite.flipX = false;
                    break;
            }
        }
    }
}