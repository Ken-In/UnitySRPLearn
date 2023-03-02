using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 存储渲染设置的资产  提供获取渲染管线实例的方法
[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")] // 在创建界面中显示
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true;
    
    // 创建渲染管线实例
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher);// 返回我们自定义的渲染管线实例
    }
}
