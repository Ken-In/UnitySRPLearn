using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 自定义的渲染管线类
public class CustomRenderPipeline : RenderPipeline
{
    bool useDynamicBatching, useGPUInstancing;
    
    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = false;
    }
    
    // 创建渲染器 来渲染每个摄像机
    private CameraRenderer cameraRenderer = new CameraRenderer();
    
    // ScriptableRenderContext 向 GPU 调度和提交状态更新和绘制命令
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            cameraRenderer.Render(context, camera, useDynamicBatching, useGPUInstancing);
        }
    }
}
