using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour {

	private Rigidbody2D rb;
	private Transform tr;
	private Animator an;

	public Transform verificaChao;
	public Transform verificaParede;
	public GameObject fire1;

	private bool estaAndando;
	private bool estaCorrendo;
	private bool estaNoChao;
	private bool estaNaParede;
	private bool estaVivo;
	private bool estaAtacando;
	private bool viradoParaDireita;
	private bool puloDuplo;
	private bool foiAcertado;
	private bool empurrandoPlayer;

	private float axis;

	public float velocidade;
	public float forcaDoPulo;
	public float raioValidaChao;
	public float raioValidaParede;

	private float velocidadeCorrida;
	private float duracaoAtaque;
	private float intervaloEntreAtaques = 0.3f;
	private float intervaloParaCorrida = 1f;
	private float duracaoAndando = 0f;
	private float forcaKnockBack = 10f;
	private float duracaoKnockBack = 0.02f;

	public LayerMask solido;

	// Use this for initialization
	void Start () {
		
		rb = GetComponent<Rigidbody2D> ();
		tr = GetComponent<Transform> ();
		an = GetComponent<Animator> ();

		fire1.SetActive (false);
		estaVivo = true;
		viradoParaDireita = true;
		velocidadeCorrida = velocidade * 2;
	}
	
	// Update is called once per frame
	void Update () {
	
		estaNoChao = Physics2D.OverlapCircle (verificaChao.position, raioValidaChao, solido);
		estaNaParede = Physics2D.OverlapCircle (verificaParede.position, raioValidaParede, solido);

		if (estaVivo) {	

			axis = Input.GetAxisRaw ("Horizontal");

			if (foiAcertado && !empurrandoPlayer) 
			{
				foiAcertado = false;
				StopCoroutine ("KnockBackPlayer");
			}

			if (!foiAcertado) {
				AttackControls ();

				RunControls ();

				WalkControls ();

				JumpControls ();
			} 
		
			Animations ();
		}	
	}

	void FixedUpdate(){
		if (estaAndando && !estaNaParede && !estaAtacando && !foiAcertado) {			
			if (viradoParaDireita)
				rb.velocity = new Vector2 (estaCorrendo ? velocidadeCorrida : velocidade, rb.velocity.y);
			else
				rb.velocity = new Vector2 (estaCorrendo ? -velocidadeCorrida : -velocidade, rb.velocity.y);
		}
	}

	void WalkControls(){
		
		estaAndando = Mathf.Abs (axis) > 0f;			 

		if ((axis > 0f && !viradoParaDireita) || (axis < 0f && viradoParaDireita)) {
			Flip ();
		}
		
	}

	void JumpControls(){

		if (Input.GetButtonDown ("Jump")) {	
			if (estaNoChao) {
				rb.AddForce (tr.up * forcaDoPulo);	
			} else if (puloDuplo) {
				rb.AddForce (tr.up * (forcaDoPulo / 1.5f));	
			}
			puloDuplo = !puloDuplo; //Habilita PuloDuplo somente se não estiver durante um PuloDuplo							
		}	
	}

	void RunControls(){
		
		duracaoAndando -= Time.deltaTime;		

		if (Input.GetButtonDown ("Horizontal") && estaNoChao) {			
			if (!(axis > 0f && !viradoParaDireita || axis < 0f && viradoParaDireita)) {

				if (duracaoAndando <= 0f) { //Aperto pela primeira vez
					duracaoAndando = intervaloParaCorrida;
					estaCorrendo = false;
				} else {	
					duracaoAndando = intervaloParaCorrida;
					estaCorrendo = true;
				}
			}
		}

		if (Input.GetButtonUp ("Horizontal") && estaCorrendo) {
			duracaoAndando = 0f;;
			estaCorrendo = false;
		}
	}

	void AttackControls(){
		
		if (Input.GetButtonDown ("Fire1") && estaNoChao && !estaAtacando) {
			estaAtacando = true;
			duracaoAtaque = intervaloEntreAtaques;
		}

		if (estaAtacando) {				
			if (duracaoAtaque > 0) {
				duracaoAtaque -= Time.deltaTime;
				if(duracaoAtaque < 0.1f) //Animação Fire1 é disparada somente ao final da animação de ataque
					fire1.SetActive (true);
			} else {
				estaAtacando = false;	
				fire1.SetActive (false);
			}
		}
	}

	void Flip(){

		viradoParaDireita = !viradoParaDireita;
		tr.localScale = new Vector2 (-transform.localScale.x, transform.localScale.y);
	
	}

	void Animations(){

		an.SetBool ("Andando", estaNoChao && estaAndando && !estaAtacando);
		an.SetBool ("Correndo", estaNoChao && estaCorrendo && !estaAtacando);
		an.SetBool ("Pulando", !estaNoChao);
		an.SetFloat ("VelVertical", rb.velocity.y);
		an.SetBool ("Atacando", estaAtacando);
		an.SetBool ("Acertado", foiAcertado);
	}

	public IEnumerator Knockback(){

		empurrandoPlayer = true;

		float timer = 0;
		while( duracaoKnockBack > timer){
			timer+=Time.deltaTime;
			rb.velocity *= -1 + forcaKnockBack;		
		}

		yield return 0;

		empurrandoPlayer = false;
	}

	void OnCollisionEnter2D(Collision2D coll)
	{		
		if (coll.gameObject.tag == "Enemy") 
		{			
			foiAcertado = true;

			estaCorrendo = false;
			estaAndando = false;
			estaAtacando = false;
			//estaNoChao = true;		

			StartCoroutine(Knockback());
		}
	}

	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red;
	    Gizmos.DrawWireSphere (verificaChao.position, raioValidaChao);
		Gizmos.DrawWireSphere (verificaParede.position, raioValidaParede);
	}
}
