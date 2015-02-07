using UnityEngine;
using System.Collections;

public class WeaponParam{
	public int magazine = 0;
	public int damage = 0;
	public float recoil = 0;//反動.
	public float mindispersion = 0;//ばらつき.
	public float dispersiongrow = 0;
	public float maxrange = 0;
	public float reloadTime = 0;
	public float interval = 0;
	public bool isReloading;
	public int load;
	public bool cooldown = false;
	public bool canShot;
}

public class WeaponComponent{
	public GameObject unit;
	public AudioSource reload;
	public PhotonView myPV;
	public WeaponControl wcontrol;
	public NormalDisplay normdisp;
	public UnitMotor motor;
}

//Base class of weapons
public abstract class Weapon : MonoBehaviour{
	protected WeaponParam param = new WeaponParam();
	protected WeaponComponent component = new WeaponComponent();

	protected void Init () {
		component.unit = transform.parent.parent.parent.gameObject;
		component.myPV = GetComponent<PhotonView>();
		component.wcontrol = component.unit.GetComponent<WeaponControl>();
		component.motor = component.unit.GetComponent<UnitMotor>();
		AudioSource[] audioSources = GetComponents<AudioSource>();
		component.reload = audioSources[0];
		param.load = param.magazine;
		param.canShot = true;
		NormalDisplay.SetReticle(component.wcontrol.dispersionRate * param.mindispersion);
		NormalDisplay.NOLtext.text = param.load.ToString();
		if (component.myPV.isMine)
			this.enabled = true;
	}

	protected void RecoilAndDisperse(){
		StopCoroutine("Recoil");
		StartCoroutine("Recoil");
		if (component.wcontrol.dispersionRate < 3f)
			component.wcontrol.dispersionRate += param.dispersiongrow;
		else
			component.wcontrol.dispersionRate = 3f;
	}

	private IEnumerator Recoil(){
		component.wcontrol.isRecoiling = true;
		float nextRecoilRotY;
		float nextRecoilRotX;
		if (component.wcontrol.recoilrotationy < 14F)
			nextRecoilRotY = component.wcontrol.recoilrotationy + param.recoil * (1f + component.wcontrol.desiredDispersion);
		else
			nextRecoilRotY = component.wcontrol.recoilrotationy - 1F;
		nextRecoilRotX = component.wcontrol.recoilrotationx + Random.Range(-param.recoil,param.recoil);
		int i = 0;
		while(i <= 6){
			component.wcontrol.recoilrotationx = Mathf.Lerp(component.wcontrol.recoilrotationx, nextRecoilRotX, 50f * Time.deltaTime);
			component.wcontrol.recoilrotationy = Mathf.Lerp(component.wcontrol.recoilrotationy, nextRecoilRotY, 50f * Time.deltaTime);
			i++;
			yield return null;
		}
		component.wcontrol.isRecoiling = false;
	}
	protected void HitMark(){
		StopCoroutine("HitMarkCoroutine");
		StartCoroutine("HitMarkCoroutine");
	}

	private IEnumerator HitMarkCoroutine(){
		NormalDisplay.RedReticle();
		yield return new WaitForSeconds(0.3f);
		NormalDisplay.WhiteReticle();
	}
	
	protected void RemainingLoads(int b){
		param.load -= b;
		if (param.load == 0){
			StartCoroutine(this.Reload ());
		}
		NormalDisplay.NOLtext.text = param.load.ToString();
	}
	
	protected virtual IEnumerator Reload(){
		component.reload.PlayOneShot(component.reload.clip);
		param.isReloading = true;
		yield return new WaitForSeconds(param.reloadTime);
		param.load = param.magazine;
		param.isReloading = false;
		NormalDisplay.NOLtext.text = param.load.ToString();
	}
}