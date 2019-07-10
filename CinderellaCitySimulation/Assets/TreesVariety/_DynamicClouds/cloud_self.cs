using UnityEngine;
using System.Collections;

public class cloud_self : MonoBehaviour 
{
    public float speed = 0.1f;

    cloud_behav cld_behav;
    ParticleSystem prt_sys;

	// Use this for initialization
	void Start () 
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);

        cld_behav = this.gameObject.transform.parent.transform.GetComponent<cloud_behav>();
        prt_sys = this.gameObject.transform.GetComponent<ParticleSystem>();

        var main = prt_sys.main;
        main.startSize = Random.Range(900, 2500);
        main.startRotationX = 0;
        main.startRotationY = 0;
        main.startRotationX = Random.Range(1, 270);
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Translate(speed, 0, speed * Time.deltaTime);

        if (space_cluster(transform.localPosition) == false)
        {
            Destroy(this.gameObject);
            cld_behav.cloud_count_cur--;
        }
	}

    bool space_cluster(Vector3 pos)
    {
        //float headx = cld_behav.gameObject.transform.localPosition.x;
        //float headz = cld_behav.gameObject.transform.localPosition.z;
        if (pos.x > 0 & pos.x < cld_behav.clusterx)
        {
            if (pos.z > 0 & pos.z < cld_behav.clusterz)
            {
                return true;
            }
        }
        return false;
    }
}
