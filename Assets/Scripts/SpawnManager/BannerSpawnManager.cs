using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BannerSpawnManager : SpawnManager
{
    enum Banner
    {
        Start,
        GameOver,
        StageClear,
    }

    class BannerBlockData : IBlockData
    {
        public int Column { get { return this.Cells.Length / this.Row; } }
        public virtual int Row { get; }
        public virtual int[] Cells { get; }
    }

    class StartBannerBlockData : BannerBlockData
    {
        public override int Row { get { return 5; } }

        public override int[] Cells { get; } = new int[]{
            1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,
            1,0,0,0,0,1,0,0,1,0,1,0,1,0,1,0,0,1,0,
            1,1,1,0,0,1,0,0,1,1,1,0,1,1,0,0,0,1,0,
            0,0,1,0,0,1,0,0,1,0,1,0,1,0,1,0,0,1,0,
            1,1,1,0,0,1,0,0,1,0,1,0,1,0,1,0,0,1,0,
        };
    }

    class TheEndBannerBlockData : BannerBlockData
    {
        public override int Row { get { return 11; } }
        public override int[] Cells { get; } = new int[]{
            1,1,1,0,1,0,1,0,1,1,1,
            0,1,0,0,1,0,1,0,1,0,0,
            0,1,0,0,1,1,1,0,1,1,1,
            0,1,0,0,1,0,1,0,1,0,0,
            0,1,0,0,1,0,1,0,1,1,1,
            0,0,0,0,0,0,0,0,0,0,0,
            1,1,1,0,1,1,1,0,1,1,0,
            1,0,0,0,1,0,1,0,1,0,1,
            1,1,1,0,1,0,1,0,1,0,1,
            1,0,0,0,1,0,1,0,1,0,1,
            1,1,1,0,1,0,1,0,1,1,0,
        };
    }

    class GoodBannerBlockData : BannerBlockData
    {
        public override int Row { get { return 5; } }
        public override int[] Cells { get; } = new int[]{
            1,1,1,0,1,1,1,0,1,1,1,0,1,1,0,
            1,0,0,0,1,0,1,0,1,0,1,0,1,0,1,
            1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,
            1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,
            1,1,1,0,1,1,1,0,1,1,1,0,1,1,0,
        };
    }

    private GameObject banner;
    private Vector3 bannerLocalPosition = new Vector3(300, 0, -200);

    // Start is called before the first frame update
    new void Start()
    {
        this.banner = new GameObject("Banner");

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PutStart(float clearTime)
    {
        this.Put(Banner.Start, clearTime);
    }

    public void PutGameOver(float clearTime)
    {
        this.Put(Banner.GameOver, clearTime);
    }

    public void PutStageClear(float clearTime)
    {
        this.Put(Banner.StageClear, clearTime);
    }

    public void ClearBanner(float clearTime)
    {
        StartCoroutine(this.Clear(clearTime));
    }

    IEnumerator Clear(float clearTime)
    {
        yield return new WaitForSeconds(clearTime);

        var velocity = 0.1f;
        var bannerTransform = this.banner.transform;
        var bound = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1, 1, 1));

        for(var i = 0; i < 1000; ++i, ++velocity)
        {
            this.banner.transform.position -= new Vector3(i * velocity, 0, 0);

            var screenPoint = Camera.main.WorldToViewportPoint(bannerTransform.position);

            if (bound.Contains(new Vector3(screenPoint.x, screenPoint.y)))
            {
                yield return new WaitForSeconds(0.001f);
            }
            else
            {
                break;
            }
        }

        foreach(Transform child in bannerTransform)
        {
            Destroy(child.gameObject);
        }
    }

    void Put(Banner banner, float clearTime)
    {
        Vector3[] cubeOffsets;

        switch (banner)
        {
            case Banner.Start:
                {
                    cubeOffsets = this.GetCubeOffsets(new StartBannerBlockData());
                    break;
                }
            case Banner.GameOver:
                {
                    cubeOffsets = this.GetCubeOffsets(new TheEndBannerBlockData());
                    break;
                }
            case Banner.StageClear:
                {
                    cubeOffsets = this.GetCubeOffsets(new GoodBannerBlockData());
                    break;
                }
            default:
                Debug.Assert(false);
                return;
        }

        this.banner.transform.localPosition = this.bannerLocalPosition;

        this.PutCubes(this.banner.transform, cubeOffsets);

        if (clearTime > 0)
        {
            StartCoroutine(this.Clear(clearTime));
        }
    }
}
