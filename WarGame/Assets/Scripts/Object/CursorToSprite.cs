using UnityEngine;
public class mouseCursor : MonoBehaviour
{
	
	private SpriteRenderer rend;
    public Sprite handCursor, normalCursor;
	
	void Start()
    {
		Cursor.visible = false;
		rend = GetComponent<SpriteRenderer>();
	}
	
	void Update ()
    {
		Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		transform.position = cursorPos;

        if (Input.GetButtonDown("Fire 1"))
            rend.sprite = handCursor;
        
        else if (Input.GetButtonUp("Fire 1"))
            rend.sprite = normalCursor;
        
	}
}