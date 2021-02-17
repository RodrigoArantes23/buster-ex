using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour {

	private Rigidbody2D rb;
	private Transform tr;
	private Animator an;
	private CircleCollider2D cc;
	private SpriteRenderer[] sr;

	public Transform verificaChao;
	public Transform verificaParede;

	private bool estaNaParede;
	private bool estaNoChao;
	private bool viradoParaDireita;

	public float velocidade;
	public float raioValidaChao;
	public float raioValidaParede;

	public LayerMask solido;

	// Use this for initialization
	void Awake () {

		rb = GetComponent<Rigidbody2D> ();
		tr = GetComponent<Transform> ();
		an = GetComponent<Animator> ();
		cc = GetComponent<CircleCollider2D> ();
		sr = GetComponentsInChildren<SpriteRenderer> ();
	
		viradoParaDireita = true;	
		Flip ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		EnemyMoviments ();
	}

	void EnemyMoviments(){	

		estaNoChao = Physics2D.OverlapCircle (verificaChao.position, raioValidaChao, solido);
		estaNaParede = Physics2D.OverlapCircle (verificaParede.position, raioValidaParede, solido);

		if ((!estaNoChao || estaNaParede) && viradoParaDireita)
			Flip ();
		else if ((!estaNoChao || estaNaParede) && !viradoParaDireita)
			Flip ();

		if (estaNoChao) {
			rb.velocity = new Vector2 (velocidade, rb.velocity.y);
		}

	}

	void Flip(){

		viradoParaDireita = !viradoParaDireita;

		tr.localScale = new Vector2 (-transform.localScale.x, transform.localScale.y);

		velocidade *= -1;

	}

	protected IEnumerator EnemyDead()
	{
		an.SetBool ("Vivo", false);
		rb.AddForce (tr.up * 500);	
		cc.enabled = false;

		Color alphaFadedColor = Color.white;
		alphaFadedColor.a = Time.realtimeSinceStartup / 10f; 
		foreach (SpriteRenderer renderer in sr)
		{
			renderer.color = alphaFadedColor;
		}

		estaNoChao = false;
		while (estaNoChao == false) 
		{
			estaNoChao = Physics2D.OverlapCircle (verificaChao.position, raioValidaChao, solido);
			yield return new WaitForSeconds(0.1f);		
		}

		Destroy (this);
	}

	void OnTriggerEnter2D(Collider2D  other){
		//rb.AddForce (tr.up * 500);	
		//rb.velocity = new Vector2 (-100000, rb.velocity.y);
		//rb.AddForce(10);
		//rb.AddForce (new Vector2(2000, 0));	

		//this.gameObject.SetActive (false);

		StartCoroutine (EnemyDead ());
		//Vector2 end = new Vector2 (200, 0);
		//Vector2 newPosition = Vector2.MoveTowards(rb.position, end, 30 * Time.deltaTime);
		//rb.MovePosition(newPosition);
	}

	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (verificaChao.position, raioValidaChao);
		Gizmos.DrawWireSphere (verificaParede.position, raioValidaParede);
	}
}
