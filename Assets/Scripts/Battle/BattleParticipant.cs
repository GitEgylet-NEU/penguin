using NohaSoftware.Utilities;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BattleParticipant : MonoBehaviour
{
	Vector3 Forward => transform.forward;
	Rigidbody rb;
	SpriteRenderer sr;
	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		sr = GetComponent<SpriteRenderer>();
	}

	public float Health { get; private set; }

	[SerializeField] bool enableHealthBar = true;
	ProgressBar healthBar;
	ProgressBar abilityBar;

	public BattleManager.Team team;
	public CharacterData Data { get; private set; }

	BattleParticipant target;

	bool canHit = true;
	bool isMovingBack = false;
	float damageSinceLastAbility;

	public void Setup(CharacterData data)
	{
		this.Data = data;
		Health = data.maxHealth;
	}


	private void Start()
	{
		if (Data == null)
		{
			if (team == BattleManager.Team.Player) Setup(BattleManager.instance.gameData.GetCharacterData("p_archer", team));
			else Setup(BattleManager.instance.gameData.GetCharacterData("e_test", team));
		}

		if (enableHealthBar)
		{
			GameObject obj = Instantiate(BattleManager.instance.healthBarPrefab, BattleManager.instance.worldSpaceCanvas.transform);
			obj.name = $"HealthBar ({name})";
			healthBar = obj.GetComponent<ProgressBar>();
			healthBar.min = 0f;
			healthBar.max = Data.maxHealth;
			healthBar.disappearOnZero = true;
			obj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		}
		if (Data.hasAbility && Data.ability.abilityCost >= 0f)
		{
			GameObject obj = Instantiate(BattleManager.instance.abilityBarPrefab, BattleManager.instance.worldSpaceCanvas.transform);
			obj.name = $"AbilityBar ({name})";
			abilityBar = obj.GetComponent<ProgressBar>();
			abilityBar.min = 0f;
			abilityBar.max = Data.ability.abilityCost;
			abilityBar.disappearOnZero = false;
			obj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		}

		BattleManager.instance.participants.Add(this);
	}

	private void Update()
	{
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
			//Debug.Log($"{gameObject.name}'s new target: {target.name}");
		}

		//rotate towards target
		float deltaAngle = Vector3.SignedAngle(Forward, (target.transform.position - transform.position).normalized, Vector3.up);
		float y;
		if (deltaAngle > 0f)
		{
			y = Mathf.Min(Data.rotationSpeed * Time.deltaTime, deltaAngle);
			if (y < 0f) y = 0f;
		}
		else
		{
			y = Mathf.Max(-Data.rotationSpeed * Time.deltaTime, deltaAngle);
			if (y > 0f) y = 0f;
		}
		rb.rotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y + y, 0f);

		//move into range
		float distance = Vector3.Distance(transform.position, target.transform.position);
		if (Data.shouldMoveBack && isMovingBack && Mathf.Abs(Data.range - distance) <= .2f) isMovingBack = false;
		if (Mathf.Abs(deltaAngle) <= 5f) //is looking at target?
		{
			if (distance > Data.range) rb.MovePosition(transform.position + Data.speed * Time.deltaTime * Forward);
			else
			{
				// move back
				if (Data.shouldMoveBack)
				{
					if (distance < Data.range * .6f) isMovingBack = true;
					if (isMovingBack) rb.MovePosition(transform.position - Data.speed * Time.deltaTime * Forward);
				}
				// hit
				if (canHit)
				{
					target.ChangeHealth(-Data.damagePerHit);
					if (Data.hasAbility) damageSinceLastAbility += Data.damagePerHit;
					//Debug.Log($"{gameObject.name} hit {target.name} for {damagePerHit} damage ({target.Health})");
					StartCoroutine(HitCooldown());
				}
			}
		}

		if (Data.hasAbility && damageSinceLastAbility >= Data.ability.abilityCost)
		{
			if (CastAbility()) damageSinceLastAbility = 0;
		}
	}

	private void LateUpdate()
	{
		if ((transform.rotation.eulerAngles.y > 90f && transform.rotation.eulerAngles.y < 270f) || (transform.rotation.eulerAngles.y < -90f && transform.rotation.eulerAngles.y > -270f))
		{
			sr.sprite = Data.frontSprite;
		}
		else
		{
			sr.sprite = Data.backSprite;
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
		yield return new WaitForSeconds(1f / Data.hitsPerSecond);
		canHit = true;
	}

	public void ChangeHealth(float amount)
	{
		Health += amount;
		if (Health <= 0) Die();
		else if (Health > Data.maxHealth) Health = Data.maxHealth;
	}
	void Die()
	{
		Debug.Log($"{gameObject.name} has died!");
		BattleManager.instance.participants.Remove(this);
		gameObject.SetActive(false);
	}

	bool CastAbility()
	{

		switch (Data.ability.id)
		{
			case "targeted_heal":
				var query = BattleManager.instance.participants.Where(p => p != this && p.team == team && p.Health <= p.Data.maxHealth);
				if (!query.Any())
				{
					if (Health <= Data.maxHealth)
					{
						Debug.Log("cast on self");
						ChangeHealth(Data.ability.floats.GetElement("heal_amount").Value);
						return true;
					}
				}
				else
				{
					var other = query.OrderBy(p => p.Health).First();
					Debug.Log("cast on " + other.name);
					other.ChangeHealth(Data.ability.floats.GetElement("heal_amount").Value);
					return true;
				}
				return false;
			default:
				return false;
		}
	}
}
