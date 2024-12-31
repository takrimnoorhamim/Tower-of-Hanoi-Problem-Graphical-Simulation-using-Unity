using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TowerOfHanoi : MonoBehaviour
{
    [Header("UI References")]
    public InputField diskCountInput;
    public InputField moveDelayInput;
    public Button playButton;
    public Text stepText;  // Add this for displaying steps

    [Header("Tower References")]
    public Transform[] towers; // Reference to the 3 tower positions

    [Header("Disk Settings")]
    public GameObject diskPrefab;
    public float diskHeight = 0.5f;
    public float minDiskWidth = 1f;
    public float diskWidthIncrement = 0.5f;

    private List<GameObject> disks = new List<GameObject>();
    private List<(int from, int to)> moves = new List<(int from, int to)>();
    private float moveDelay = 1f;
    private bool isAnimating = false;
    private int currentStep = 0;
    private int totalSteps = 0;

    void Start()
    {
        playButton.onClick.AddListener(StartSimulation);
        
    }

    void StartSimulation()
    {
        if (isAnimating) return;

        // Clear previous simulation
        foreach (var disk in disks)
        {
            Destroy(disk);
        }
        disks.Clear();
        moves.Clear();

        // Get input values
        if (!int.TryParse(diskCountInput.text, out int diskCount))
            diskCount = 3;

        if (!float.TryParse(moveDelayInput.text, out moveDelay))
            moveDelay = 1f;

        // Create disks
        CreateDisks(diskCount);

        // Calculate moves
        CalculateMoves(diskCount, 0, 2, 1);

        // Start animation
        StartCoroutine(AnimateMoves());
    }

    void CreateDisks(int count)
    {
        for (int i = count - 1; i >= 0; i--)
        {
            GameObject disk = Instantiate(diskPrefab);
            float width = minDiskWidth + i * diskWidthIncrement;
            disk.transform.localScale = new Vector3(width, diskHeight, width);

            // Position disk on first tower starting from absolute bottom
            Vector3 position = towers[0].position;
            position.y = 0 + ((count - 1 - i) * diskHeight);  // Start from y=0 and stack up

            // Ensure disk is centered on the tower
            disk.transform.position = position;
            disk.transform.SetParent(towers[0], true);

            disks.Add(disk);
        }
    }

    void CalculateMoves(int n, int from, int to, int aux)
    {
        if (n == 1)
        {
            moves.Add((from, to));
            return;
        }

        CalculateMoves(n - 1, from, aux, to);
        moves.Add((from, to));
        CalculateMoves(n - 1, aux, to, from);

        totalSteps = moves.Count;
    }

    IEnumerator AnimateMoves()
    {
        isAnimating = true;
        currentStep = 0;

        // Update initial step text
        if (stepText != null)
        {
            stepText.text = $"Step 0/{totalSteps}";
        }

        foreach (var move in moves)
        {
            yield return new WaitForSeconds(moveDelay);

            // Find the top disk in the 'from' tower
            GameObject diskToMove = null;
            float maxHeight = float.MinValue;

            foreach (var disk in disks)
            {
                Vector3 diskPos = disk.transform.position;
                if (Mathf.Approximately(diskPos.x, towers[move.from].position.x) &&
                    diskPos.y > maxHeight)
                {
                    maxHeight = diskPos.y;
                    diskToMove = disk;
                }
            }

            if (diskToMove != null)
            {
                // Calculate target position
                Vector3 targetPos = towers[move.to].position;
                float targetHeight = 0f;

                // Find height for the disk on target tower
                foreach (var disk in disks)
                {
                    Vector3 diskPos = disk.transform.position;
                    if (Mathf.Approximately(diskPos.x, targetPos.x))
                    {
                        targetHeight = Mathf.Max(targetHeight, diskPos.y + diskHeight);
                    }
                }

                targetPos.y = targetHeight;

                // Animate the move
                StartCoroutine(MoveDisk(diskToMove, targetPos));
            }
        }

        isAnimating = false;
    }

    IEnumerator MoveDisk(GameObject disk, Vector3 targetPosition)
    {
        currentStep++;
        if (stepText != null)
        {
            stepText.text = $"Steps: Step {currentStep}/{totalSteps}";
        }

        Vector3 startPos = disk.transform.position;
        Vector3 midPos = startPos + Vector3.up * 2f; // Lift disk up

        // Move up
        float elapsed = 0f;
        float duration = moveDelay * 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            disk.transform.position = Vector3.Lerp(startPos, midPos, t);
            yield return null;
        }

        // Move horizontally
        Vector3 midTargetPos = new Vector3(targetPosition.x, midPos.y, midPos.z);
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            disk.transform.position = Vector3.Lerp(midPos, midTargetPos, t);
            yield return null;
        }

        // Move down
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            disk.transform.position = Vector3.Lerp(midTargetPos, targetPosition, t);
            yield return null;
        }

        disk.transform.position = targetPosition;
    }
}