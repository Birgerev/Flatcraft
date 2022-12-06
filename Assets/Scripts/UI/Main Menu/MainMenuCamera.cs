using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    public float xSpeed;
    public float maxX;

    // Update is called once per frame
    private void Update()
    {
        MoveCamera();

        if (transform.position.x >= maxX)
            ReturnToStartPoint();
    }

    private void ReturnToStartPoint()
    {
        transform.position -= new Vector3(maxX, 0);
    }

    private void MoveCamera()
    {
        transform.position += new Vector3(xSpeed * Time.deltaTime, 0);
    }
}