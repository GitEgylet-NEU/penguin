using NohaSoftware.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleParticipant : MonoBehaviour
{
	Vector3 Forward => fwMultiplier * transform.forward;
	float fwMultiplier = 1f;
	Rigidbody rb;
	SpriteRenderer sr;
	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		sr = GetComponent<SpriteRenderer>();
	}

	public float Health { get; private set; }
	public float XpYield { get; private set; } = -1f;

	[SerializeField] bool enableHealthBar = true;
	ProgressBar healthBar;
	ProgressBar abilityBar;

	public BattleManager.Team team;
	public BattleParticipantData Data { get; private set; }
	public BattleParticipantData.Level Level { get; private set; }

	BattleParticipant target;

	bool canHit = true;
	//bool isMovingBack = false;
	float damageSinceLastAbility;

	bool run = false;

	public void Setup(BattleParticipantData data)
	{
		this.Data = data;
		if (team == BattleManager.Team.Player) Level = data.levels[SaveManager.instance.progressData.characterLevels.GetElement(data.name).Value];
		else Level = data.levels[0];
		Health = Level.maxHealth;
		run = true;

		if (data.GetType() == typeof(EnemyData)) XpYield = ((EnemyData)data).xpYield;

		if (team == BattleManager.Team.Enemy) fwMultiplier = -1f;
	}


	private void Start()
	{
		if (Data == null) return;

		if (enableHealthBar)
		{
			GameObject obj = Instantiate(BattleManager.instance.healthBarPrefab, BattleManager.instance.worldSpaceCanvas.transform);
			obj.name = $"HealthBar ({name})";
			healthBar = obj.GetComponent<ProgressBar>();
			healthBar.min = 0f;
			healthBar.max = Level.maxHealth;
			healthBar.disappearOnZero = true;
			obj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		}
		if (Level.hasAbility && Level.ability.abilityCost >= 0f)
		{
			GameObject obj = Instantiate(BattleManager.instance.abilityBarPrefab, BattleManager.instance.worldSpaceCanvas.transform);
			obj.name = $"AbilityBar ({name})";
			abilityBar = obj.GetComponent<ProgressBar>();
			abilityBar.min = 0f;
			abilityBar.max = Level.ability.abilityCost;
			abilityBar.disappearOnZero = false;
			obj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		}

		BattleManager.instance.participants.Add(this);
	}

	public void DisableParticipant()
	{
		run = false;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}

	Coroutine lookAtCoroutine;
	float lookAtThreshold = 5f;
	void StartLookAt()
	{
		if (lookAtCoroutine != null)
		{
			StopCoroutine(lookAtCoroutine);
		}
		lookAtCoroutine = StartCoroutine(LookAt());
	}
	IEnumerator LookAt()
	{
		float t = 0f;
		while (t < 1f)
		{
			rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(fwMultiplier * (target.rb.position - rb.position)), t);
			t += Time.deltaTime * .2f;
			yield return null;
		}
		lookAtCoroutine = null;
	}

	private void Update()
	{
		if (!run) return;

		if (healthBar != null)
		{
			healthBar.SetValue(Health);
			healthBar.transform.position = transform.position + new Vector3(0f, transform.localScale.y / 2f + .35f, 0f);
		}
		if (abilityBar != null)
		{
			abilityBar.SetValue(damageSinceLastAbility);
			abilityBar.transform.position = transform.position + new Vector3(0f, transform.localScale.y / 2f + .15f, 0f);
		}

		if (target == null || target.Health <= 0f)
		{
			target = null;
			target = BattleManager.instance.participants.Where(p => p.team != team).OrderBy(p => Vector3.Distance(transform.position, p.transform.position)).FirstOrDefault();
			if (target == null) return;
			StartLookAt();
			//Debug.Log($"{gameObject.name}'s new target: {target.name}");
		}

		//rotate towards target
		float deltaAngle = Vector3.SignedAngle(Forward, target.transform.position - transform.position, Vector3.up);
		if (lookAtCoroutine == null && (Mathf.Abs(deltaAngle) >= lookAtThreshold || Mathf.Abs(180f - Mathf.Abs(deltaAngle)) >= lookAtThreshold))
		{
			StartLookAt();
		}

		//move into range
		float distance = Vector3.Distance(transform.position, target.transform.position);
		//if (Level.shouldMoveBack && isMovingBack && Mathf.Abs(Level.range - distance) <= .2f) isMovingBack = false;
		if (180f - Mathf.Abs(deltaAngle) <= lookAtThreshold || Mathf.Abs(deltaAngle) <= lookAtThreshold) //is looking at target?
		{
			if (distance > Level.range) rb.MovePosition(transform.position + Data.speed * Time.deltaTime * Forward);
			else
			{
				// move back
				//if (Level.shouldMoveBack)
				//{
				//	if (distance < Level.range * .6f) isMovingBack = true;
				//	if (isMovingBack) rb.MovePosition(transform.position - Level.speed * Time.deltaTime * Forward);
				//}
				// hit
				if (canHit)
				{
					target.ChangeHealth(-Level.damagePerHit);
					if (Level.hasAbility) damageSinceLastAbility += Level.damagePerHit;
					//Debug.Log($"{gameObject.name} hit {target.name} for {damagePerHit} damage ({target.Health})");
					StartCoroutine(HitCooldown());
				}
			}
		}

		if (Level.hasAbility && damageSinceLastAbility >= Level.ability.abilityCost)
		{
			if (CastAbility()) damageSinceLastAbility = 0;
		}
	}

	private void LateUpdate()
	{
		if (!run) return;
		if ((transform.rotation.eulerAngles.y > 90f && transform.rotation.eulerAngles.y < 270f) || (transform.rotation.eulerAngles.y < -90f && transform.rotation.eulerAngles.y > -270f))
		{
			if (fwMultiplier == 1f) sr.sprite = Data.frontSprite;
			else sr.sprite = Data.backSprite;
		}
		else
		{
			if (fwMultiplier == 1f) sr.sprite = Data.backSprite;
			else sr.sprite = Data.frontSprite;
		}
	}

	private void OnEnable()
	{
		if (healthBar != null) healthBar.gameObject.SetActive(true);
	}
	private void OnDisable()
	{
		if (healthBar != null) healthBar.gameObject.SetActive(false);
	}
	private void OnDestroy()
	{
		if (healthBar != null) Destroy(healthBar.gameObject);
	}

	IEnumerator HitCooldown()
	{
		canHit = false;
		yield return new WaitForSeconds(1f / Level.hitsPerSecond);
		canHit = true;
	}

	public void ChangeHealth(float amount)
	{
		Health += amount;
		if (Health <= 0) Die();
		else if (Health > Level.maxHealth) Health = Level.maxHealth;
	}
	void Die()
	{
		Debug.Log($"{gameObject.name} has died!");
		BattleManager.instance.participants.Remove(this);
		if (abilityBar != null) Destroy(abilityBar.gameObject);

		if (XpYield != -1f) BattleManager.instance.AddXP(XpYield);

		Destroy(gameObject);
	}

	bool CastAbility()
	{
		IEnumerable<BattleParticipant> query;
		switch (Level.ability.id)
		{
			case "targeted_heal":
				query = BattleManager.instance.participants.Where(p => p != this && p.team == team && p.Health <= p.Level.maxHealth);
				if (!query.Any())
				{
					if (Health <= Level.maxHealth)
					{
						ChangeHealth(Level.ability.floats.GetElement("heal_amount").Value);
						ShowParticle(BattleManager.instance.gameData.visualData.abilityParticles.GetElement(Level.ability.id).Value, 1f);
						return true;
					}
				}
				else
				{
					var other = query.OrderBy(p => p.Health / p.Level.maxHealth).First();
					other.ChangeHealth(Level.ability.floats.GetElement("heal_amount").Value);
					other.ShowParticle(BattleManager.instance.gameData.visualData.abilityParticles.GetElement(Level.ability.id).Value, 1f);
					return true;
				}
				return false;
			case "area_damage":
				float range = Level.ability.floats.GetElement("range").Value;
				query = BattleManager.instance.participants.Where(p => p.team != team && Vector3.Distance(transform.position, p.transform.position) <= range);
				if (!query.Any()) return false;
				foreach (var target in query)
				{
					target.ChangeHealth(-Level.ability.floats.GetElement("damage").Value);
				}
				return true;
			default:
				return false;
		}
	}

	public void ShowParticle(GameObject particle, float time = 0f)
	{
		GameObject obj = Instantiate(particle, transform);
		ParticleSystem sys = obj.GetComponent<ParticleSystem>();
		sys.Play();

		IEnumerator StopParticle()
		{
			yield return new WaitForSeconds(time);
			sys.Stop();
		}

		if (time > 0f) StartCoroutine(StopParticle());
	}
}
