using UnityEngine;
using System.Collections;

public class cloud_behav : MonoBehaviour 
{
    public GameObject cloud_1;
    public GameObject cloud_2;
    public GameObject cloud_3;

    public float clusterx;
    public float clustery;
    public float clusterz;
    public int cloud_count_max = 10;
    public int cloud_count_cur;
    public int spawn_interval = 3;

    bool spawn = true;
    GameObject new_cloud;

    //
    void Awake()
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);
    }

	// Use this for initialization
	void Start () 
    {
        for (int k = 0; k <= cloud_count_max; k++)
        {
            spawn_cloud(1);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (spawn)
        {
            StartCoroutine(spawn_cldwn());
            spawn = false;
            spawn_cloud(0);
        }
	}

    void spawn_cloud(int mode)
    {
        if (cloud_count_cur < cloud_count_max)
        {
            switch (Random.Range(1, 4))
            {
                case 1: new_cloud = Instantiate(cloud_1); break;
                case 2: new_cloud = Instantiate(cloud_2); break;
                case 3: new_cloud = Instantiate(cloud_3); break;
            }
            new_cloud.transform.SetParent(this.gameObject.transform, true);
            new_cloud.transform.localPosition = start_cloud_pos(mode);
            cloud_count_cur++;
        }
    }

    IEnumerator spawn_cldwn()
    {
        yield return new WaitForSeconds(spawn_interval);
        spawn = true;
    }

    Vector3 start_cloud_pos(int mode)
    {
        float x = 0;
        if (mode == 1) x = Random.Range(1, clusterx);
        else x = 10;
        float y = Random.Range(1, clustery);
        float z = Random.Range(1, clusterz);
        Vector3 new_pos = new Vector3(x, y, z);
        return new_pos;
    }
}
