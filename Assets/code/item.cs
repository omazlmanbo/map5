using UnityEngine;
using System.Collections;

public class CollectibleItem2D : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            // 在销毁前显示文字
            ShowText("拾取后鼠标左键攻击");
            
            Destroy(gameObject);
        }
    }

    void ShowText(string text)
    {
        Vector3 basePosition = transform.position;
        
        // 创建主文字对象（红色）
        GameObject mainTextObj = new GameObject("FloatingText_Main");
        mainTextObj.transform.position = basePosition;
        TextMesh mainTextMesh = mainTextObj.AddComponent<TextMesh>();
        mainTextMesh.text = text;
        mainTextMesh.fontSize = 2;
        mainTextMesh.color = Color.red;
        mainTextMesh.anchor = TextAnchor.MiddleCenter;
        mainTextMesh.alignment = TextAlignment.Center;
        MeshRenderer mainRenderer = mainTextObj.GetComponent<MeshRenderer>();
        if (mainRenderer != null)
        {
            mainRenderer.sortingLayerName = "Default";
            mainRenderer.sortingOrder = 102;
        }
        
        // 创建RGB通道分离效果（故障艺术风格）
        // 绿色通道偏移
        GameObject greenTextObj = new GameObject("FloatingText_Green");
        greenTextObj.transform.position = basePosition + new Vector3(0.05f, 0, 0);
        TextMesh greenTextMesh = greenTextObj.AddComponent<TextMesh>();
        greenTextMesh.text = text;
        greenTextMesh.fontSize = 2;
        greenTextMesh.color = new Color(0, 1, 0, 0.5f); // 半透明绿色
        greenTextMesh.anchor = TextAnchor.MiddleCenter;
        greenTextMesh.alignment = TextAlignment.Center;
        MeshRenderer greenRenderer = greenTextObj.GetComponent<MeshRenderer>();
        if (greenRenderer != null)
        {
            greenRenderer.sortingLayerName = "Default";
            greenRenderer.sortingOrder = 101;
        }
        
        // 蓝色通道偏移
        GameObject blueTextObj = new GameObject("FloatingText_Blue");
        blueTextObj.transform.position = basePosition + new Vector3(-0.05f, 0, 0);
        TextMesh blueTextMesh = blueTextObj.AddComponent<TextMesh>();
        blueTextMesh.text = text;
        blueTextMesh.fontSize = 2;
        blueTextMesh.color = new Color(0, 0, 1, 0.5f); // 半透明蓝色
        blueTextMesh.anchor = TextAnchor.MiddleCenter;
        blueTextMesh.alignment = TextAlignment.Center;
        MeshRenderer blueRenderer = blueTextObj.GetComponent<MeshRenderer>();
        if (blueRenderer != null)
        {
            blueRenderer.sortingLayerName = "Default";
            blueRenderer.sortingOrder = 100;
        }
        
        // 创建一个辅助对象来运行协程（因为原物体会被销毁）
        GameObject coroutineRunner = new GameObject("TextCoroutineRunner");
        FloatingTextController controller = coroutineRunner.AddComponent<FloatingTextController>();
        controller.StartFloating(mainTextObj, greenTextObj, blueTextObj);
    }
}

// 辅助类用于运行文字浮动协程
public class FloatingTextController : MonoBehaviour
{
    public void StartFloating(GameObject mainTextObj, GameObject greenTextObj, GameObject blueTextObj)
    {
        StartCoroutine(FloatText(mainTextObj, greenTextObj, blueTextObj));
    }

    IEnumerator FloatText(GameObject mainTextObj, GameObject greenTextObj, GameObject blueTextObj)
    {
        TextMesh mainTextMesh = mainTextObj.GetComponent<TextMesh>();
        TextMesh greenTextMesh = greenTextObj.GetComponent<TextMesh>();
        TextMesh blueTextMesh = blueTextObj.GetComponent<TextMesh>();
        
        Vector3 mainStartPos = mainTextObj.transform.position;
        Vector3 greenStartPos = greenTextObj.transform.position;
        Vector3 blueStartPos = blueTextObj.transform.position;
        
        float duration = 4f;
        float elapsed = 0f;
        
        // 故障艺术效果参数
        float glitchIntensity = 0.1f;
        float glitchFrequency = 0.1f;
        float lastGlitchTime = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // 故障艺术效果：随机抖动和RGB通道分离
            if (Time.time - lastGlitchTime > glitchFrequency)
            {
                lastGlitchTime = Time.time;
                
                // 主文字位置随机抖动
                Vector3 glitchOffset = new Vector3(
                    Random.Range(-glitchIntensity, glitchIntensity),
                    Random.Range(-glitchIntensity, glitchIntensity),
                    0
                );
                mainTextObj.transform.position = mainStartPos + Vector3.up * progress * 2f + glitchOffset;
                
                // RGB通道分离效果（动态偏移）
                float channelOffset = Random.Range(-0.1f, 0.1f);
                greenTextObj.transform.position = greenStartPos + Vector3.up * progress * 2f + 
                    new Vector3(0.05f + channelOffset, 0, 0);
                blueTextObj.transform.position = blueStartPos + Vector3.up * progress * 2f + 
                    new Vector3(-0.05f - channelOffset, 0, 0);
                
                // 颜色闪烁效果（红色主文字）
                float flicker = Random.Range(0.7f, 1f);
                mainTextMesh.color = new Color(flicker, 0, 0, 1f - progress);
                
                // 绿色和蓝色通道也闪烁
                greenTextMesh.color = new Color(0, flicker * 0.5f, 0, (1f - progress) * 0.5f);
                blueTextMesh.color = new Color(0, 0, flicker * 0.5f, (1f - progress) * 0.5f);
            }
            else
            {
                // 正常向上移动
                mainTextObj.transform.position = mainStartPos + Vector3.up * progress * 2f;
                greenTextObj.transform.position = greenStartPos + Vector3.up * progress * 2f;
                blueTextObj.transform.position = blueStartPos + Vector3.up * progress * 2f;
                
                // 淡出效果
                mainTextMesh.color = new Color(1f, 0, 0, 1f - progress);
                greenTextMesh.color = new Color(0, 0.5f, 0, (1f - progress) * 0.5f);
                blueTextMesh.color = new Color(0, 0, 0.5f, (1f - progress) * 0.5f);
            }
            
            yield return null;
        }
        
        // 销毁文字对象和协程运行器
        Destroy(mainTextObj);
        Destroy(greenTextObj);
        Destroy(blueTextObj);
        Destroy(gameObject);
    }
}
