using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public class FireEffect : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float burnLevel = 0f;
        [Header("Testing Options:")]
        public bool onFire = false;
        public float burnSpeed = 0.1f;
        //public ParticleSystem fireParticles;
        [Header("Tagged with 'Charable':")]
        public List<MeshRenderer> charables = new List<MeshRenderer>();
        public List<Material> charableMaterials = new List<Material>();
        private List<Material> charableMatsNEW = new List<Material>();
        private List<Color> charableStartingColours = new List<Color>();
        [Header("Tagged with 'Burnable':")]
        public List<BurnableObj> burnables = new List<BurnableObj>();
        public List<Material> burnableMaterials = new List<Material>();
        private List<Material> burnableMatsNEW = new List<Material>();
        private List<Color> burnableStartingColours = new List<Color>();

        [Header("Settings")]
        public bool regrow = false;
        [Range(0.5f, 1f)]
        public float burnAwayThreshold = 0.9f;
        [Range(0f, 0.5f)]
        public float regrowThreshold = 0.1f;
        [Min(0.1f)]
        public float dropSpeed = 5f;
        [Min(0.1f)]
        public float growTime = 3f;
        [Min(0f)]
        public float animationStartTimeRange = 3f;
        private float _oldBurntLevel = 0f;
        private bool _materialsSetup = false;
        private bool _fullyBurnt = false;

        private const float burnableFallDist = 10f;

        // Start is called before the first frame update
        void Start()
        {
            if (burnables.Count == 0 || charables.Count == 0)
                GetBurnables();
        }

        [ContextMenu("Setup Burnables")]
        public void GetBurnables()
        {
            Resources.UnloadUnusedAssets();

            MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
            SetUpBurnables(meshes);
        }


        public void GiveBurnables(MeshRenderer[] meshes)
        {
            SetUpBurnables(meshes);
        }

        private void SetUpBurnables(MeshRenderer[] meshes)
        {
            charables.Clear();
            charableMaterials.Clear();
            burnables.Clear();
            burnableMatsNEW.Clear();
            int index = 0;
            foreach (MeshRenderer MR in meshes)
            {
                switch (MR.gameObject.tag)
                {
                    case "Charable":
                        charables.Add(MR);
                        index = charableMaterials.IndexOf(MR.sharedMaterial);
                        if (index == -1)
                        {
                            charableMaterials.Add(MR.sharedMaterial);
                        }
                        break;
                    case "Burnable":
                        burnables.Add(new BurnableObj(MR));
                        index = burnableMaterials.IndexOf(MR.sharedMaterial);
                        if (index == -1)
                        {
                            burnableMaterials.Add(MR.sharedMaterial);
                        }
                        break;
                }
            }
        }

        protected int IndexOfMatchingMat(List<Material> list, Material mat)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] == mat)
                    return i;
            }
            return -1;
        }

        private void SetupMaterials()
        {
            charableMatsNEW.Clear();
            charableStartingColours.Clear();
            foreach (Material mat in charableMaterials)
            {
                charableMatsNEW.Add(new Material(mat));
                charableStartingColours.Add(mat.color);
            }

            int index;
            foreach (MeshRenderer charable in charables)
            {
                index = charableMaterials.IndexOf(charable.sharedMaterial);
                if (index == -1)
                    Debug.LogError("Material " + charable.sharedMaterial.name + " not setup");
                else
                    charable.sharedMaterial = charableMatsNEW[index];
            }

            burnableMatsNEW.Clear();
            burnableStartingColours.Clear();
            foreach (Material mat in burnableMaterials)
            {
                burnableMatsNEW.Add(new Material(mat));
                burnableStartingColours.Add(mat.color);
            }

            foreach (BurnableObj burnable in burnables)
            {
                index = burnableMaterials.IndexOf(burnable.mesh.sharedMaterial);
                if (index == -1)
                    Debug.LogError("Material " + burnable.mesh.sharedMaterial.name + " not setup");
                else
                    burnable.mesh.sharedMaterial = burnableMatsNEW[index];
            }

            SetupBurnablesData();

            _materialsSetup = true;
        }

        private void SetupBurnablesData()
        {
            foreach (BurnableObj burnable in burnables)
                burnable.RefreshData(animationStartTimeRange);
        }

        // Update is called once per frame
        void Update()
        {
            TestUpdate();
        }

        private void TestUpdate()
        {
            if (onFire)
            {

                if (burnLevel < 1f)
                    burnLevel += burnSpeed * Time.deltaTime;
            }
            else
            {
                if (regrow && burnLevel > 0f)
                    burnLevel -= burnSpeed * Time.deltaTime;

                //if (fireParticles.isPlaying)
                //    fireParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }

            //float fSize = Mathf.Min(0.5f + burnLevel * 2f, 1f);
            //fireParticles.transform.localScale = new Vector3(fSize, fSize, fSize);

            SetBurntness(burnLevel);
        }

        public void SetBurntness(float value01)
        {
            if (_materialsSetup == false)
                SetupMaterials();

            if (charableMatsNEW.Count == 0 && burnableMatsNEW.Count == 0)
                return;

            value01 = Mathf.Clamp01(value01);

            if ((regrow && value01 == _oldBurntLevel) || (regrow == false && value01 < _oldBurntLevel))
                return;

            //Fade charables and burnables between their original colour and black
            float vibrance = 1f - value01;

            for (int i = 0; i < charableMatsNEW.Count; ++i)
                charableMatsNEW[i].color = charableStartingColours[i].MultiplyVibrance(vibrance);

            for (int i = 0; i < burnableMatsNEW.Count; ++i)
                burnableMatsNEW[i].color = burnableStartingColours[i].MultiplyVibrance(vibrance);

            //If burn value is above threshold, make burnables fall down
            if (value01 >= burnAwayThreshold && _oldBurntLevel < burnAwayThreshold)
                DropBurnables();

            //If burn value is below threshold, make burnables regrow
            if (regrow && value01 <= regrowThreshold && _oldBurntLevel > regrowThreshold)
                RegrowBurnables();

            _oldBurntLevel = value01;
        }

        private void DropBurnables()
        {
            if (_dropAnimation == null && _fullyBurnt == false)
                _dropAnimation = StartCoroutine(BurnablesDropAnimation());
        }

        private Coroutine _dropAnimation;
        private IEnumerator BurnablesDropAnimation()
        {
            float startTime = Time.time;
            float maxAnimationDur = 5f;
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            bool allAnimationsFinished = false;

            //Mark all animations as unfinished
            foreach (BurnableObj burnable in burnables)
                burnable.animationFinished = false;

            //Calculate animations until all are finished or the max time is up.
            while (allAnimationsFinished == false || Time.time < startTime + maxAnimationDur)
            {
                allAnimationsFinished = true;
                //Loop through each burnable, and if it hasn't finished its animation, move it down and mark that at least one animation is not finished.
                foreach (BurnableObj burnable in burnables)
                {
                    if (burnable.animationFinished == true)
                        continue;
                    if (Time.time > startTime + burnable.animationDelay)
                    {
                        Vector3 targetPos = burnable.originalWorldPos + Vector3.down * burnableFallDist;
                        burnable.transform.position = Vector3.MoveTowards(burnable.transform.position, targetPos, dropSpeed * Time.deltaTime);
                        if (burnable.transform.position == targetPos)
                        {
                            burnable.animationFinished = true;
                            continue;
                        }
                    }
                    allAnimationsFinished = false;
                }
                yield return wait;
            }

            _fullyBurnt = true;
            _dropAnimation = null;
        }

        private Coroutine _growAnimation;
        private void RegrowBurnables()
        {
            if (_growAnimation == null && _fullyBurnt)
                _growAnimation = StartCoroutine(BurnablesRegrowAnimation());
        }

        private IEnumerator BurnablesRegrowAnimation()
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            if (_dropAnimation != null)
                yield return wait;

            foreach (BurnableObj burnable in burnables)
            {
                burnable.transform.localScale = Vector3.zero;
                burnable.transform.position = burnable.originalWorldPos;
                burnable.animationFinished = false;
            }

            float startTime = Time.time;
            float maxAnimationDur = 10f;
            bool allAnimationsFinished = false;
            while (allAnimationsFinished == false && Time.time < startTime + maxAnimationDur)
            {
                allAnimationsFinished = true;
                //Loop through each burnable, and if it hasn't finished its animation, move it down and mark that at least one animation is not finished.
                foreach (BurnableObj burnable in burnables)
                {
                    if (burnable.animationFinished == true)
                        continue;
                    float growth = (Time.time - (startTime + burnable.animationDelay)) / growTime;
                    if (growth > 0)
                    {
                        burnable.transform.localScale = Vector3.Lerp(burnable.transform.localScale, burnable.originalLocalScale, growth);
                        if (burnable.transform.localScale == burnable.originalLocalScale)
                        {
                            burnable.animationFinished = true;
                            continue;
                        }
                    }
                    allAnimationsFinished = false;
                }
                yield return wait;
            }

            _fullyBurnt = false;
            _growAnimation = null;
        }


        [System.Serializable]
        public class BurnableObj
        {
            public const float animationDelayRange = 3f;

            public MeshRenderer mesh;
            public Transform transform;
            public Vector3 originalWorldPos;
            public Vector3 originalLocalScale;
            public float animationDelay = 0f;
            public bool animationFinished = false;

            public BurnableObj(MeshRenderer MR)
            {
                mesh = MR;
                transform = MR.transform;
                originalWorldPos = transform.position;
                originalLocalScale = transform.localScale;
                animationDelay = Random.value * animationDelayRange;
            }

            public void RefreshData(float animationRange)
            {
                transform = mesh.transform;
                originalWorldPos = transform.position;
                originalLocalScale = transform.localScale;
                animationDelay = Random.value * animationRange;
            }
        }
    }

    [System.Serializable]
    public class BurntEffect
    {
        [Range(0f, 1f)]
        public float burntLevel = 0f;

        [Header("Tagged with 'Charable':")]
        public List<MeshRenderer> charables = new List<MeshRenderer>();
        public List<Material> charableMaterials = new List<Material>();
        private List<Material> charableMatsNEW = new List<Material>();
        private List<Color> charableStartingColours = new List<Color>();

        [Header("Tagged with 'Burnable':")]
        public List<BurnableObj> burnables = new List<BurnableObj>();
        public List<Material> burnableMaterials = new List<Material>();
        private List<Material> burnableMatsNEW = new List<Material>();
        private List<Color> burnableStartingColours = new List<Color>();

        [Header("Settings")]
        public bool regrow = false;
        [Range(0.5f, 1f)]
        public static float burnAwayThreshold = 0.9f;
        [Range(0f, 0.5f)]
        public static float regrowThreshold = 0.1f;
        [Min(0.1f)]
        public static float dropSpeed = 20f;
        [Min(0.1f)]
        public static float growTime = 3f;
        [Min(0f)]
        public static float animationStartTimeRange = 1f;
        [Range(0f, 1f)]
        public static float burntVibrance = 0.1f;
        private float _oldBurntLevel = 0f;
        private bool _materialsSetup = false;
        private bool _fullyBurnt = false;

        private static float burnableFallDist = 30f;

        public void SetUpBurnables(MeshRenderer[] meshes)
        {
            charables.Clear();
            charableMaterials.Clear();
            burnables.Clear();
            burnableMatsNEW.Clear();
            int index = 0;
            foreach (MeshRenderer MR in meshes)
            {
                switch (MR.gameObject.tag)
                {
                    case "Charable":
                        charables.Add(MR);
                        index = charableMaterials.IndexOf(MR.sharedMaterial);
                        if (index == -1)
                        {
                            charableMaterials.Add(MR.sharedMaterial);
                        }
                        break;
                    case "Burnable":
                        burnables.Add(new BurnableObj(MR));
                        index = burnableMaterials.IndexOf(MR.sharedMaterial);
                        if (index == -1)
                        {
                            burnableMaterials.Add(MR.sharedMaterial);
                        }
                        break;
                }
            }
        }

        protected int IndexOfMatchingMat(List<Material> list, Material mat)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] == mat)
                    return i;
            }
            return -1;
        }

        private void SetupMaterials()
        {
            charableMatsNEW.Clear();
            charableStartingColours.Clear();
            foreach (Material mat in charableMaterials)
            {
                charableMatsNEW.Add(new Material(mat));
                charableStartingColours.Add(mat.color);
            }

            int index;
            foreach (MeshRenderer charable in charables)
            {
                index = charableMaterials.IndexOf(charable.sharedMaterial);
                if (index == -1)
                    Debug.LogError("Material " + charable.sharedMaterial.name + " not setup");
                else
                    charable.sharedMaterial = charableMatsNEW[index];
            }

            burnableMatsNEW.Clear();
            burnableStartingColours.Clear();
            foreach (Material mat in burnableMaterials)
            {
                burnableMatsNEW.Add(new Material(mat));
                burnableStartingColours.Add(mat.color);
            }

            foreach (BurnableObj burnable in burnables)
            {
                index = burnableMaterials.IndexOf(burnable.mesh.sharedMaterial);
                if (index == -1)
                    Debug.LogError("Material " + burnable.mesh.sharedMaterial.name + " not setup");
                else
                    burnable.mesh.sharedMaterial = burnableMatsNEW[index];
            }

            SetupBurnablesData();

            _materialsSetup = true;
        }

        private void SetupBurnablesData()
        {
            foreach (BurnableObj burnable in burnables)
                burnable.RefreshData(animationStartTimeRange);
        }

        public void SetBurntness(float value01)
        {
            if (_materialsSetup == false)
                SetupMaterials();

            if (charableMatsNEW.Count == 0 && burnableMatsNEW.Count == 0)
                return;

            burntLevel = Mathf.Clamp01(value01);

            if ((regrow && burntLevel == _oldBurntLevel) || (regrow == false && burntLevel < _oldBurntLevel))
                return;

            //Fade charables and burnables between their original colour and black
            float vibrance = 1f - (burntLevel * (1f - burntVibrance));

            for (int i = 0; i < charableMatsNEW.Count; ++i)
                charableMatsNEW[i].color = charableStartingColours[i].MultiplyVibrance(vibrance);

            for (int i = 0; i < burnableMatsNEW.Count; ++i)
                burnableMatsNEW[i].color = burnableStartingColours[i].MultiplyVibrance(vibrance);

            _oldBurntLevel = burntLevel;
        }

        public bool CanStartDropAnimation()
        {
            return (burntLevel >= burnAwayThreshold)
                && (_dropAnimation == null && _fullyBurnt == false);
        }

        private Coroutine _dropAnimation;
        public Coroutine DropCoroutine { set => _dropAnimation = value; }
        public static IEnumerator BurnablesDropAnimation(BurntEffect burnEffect)
        {
            float startTime = Time.time;
            float maxAnimationDur = 5f;
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            bool allAnimationsFinished = false;

            //Mark all animations as unfinished
            foreach (BurnableObj burnable in burnEffect.burnables)
                burnable.animationFinished = false;

            //Calculate animations until all are finished or the max time is up.
            while (allAnimationsFinished == false || Time.time < startTime + maxAnimationDur)
            {
                allAnimationsFinished = true;
                //Loop through each burnable, and if it hasn't finished its animation, move it down and mark that at least one animation is not finished.
                foreach (BurnableObj burnable in burnEffect.burnables)
                {
                    if (burnable.animationFinished == true)
                        continue;
                    if (Time.time > startTime + burnable.animationDelay)
                    {
                        Vector3 targetPos = burnable.originalWorldPos + Vector3.down * burnableFallDist;
                        burnable.transform.position = Vector3.MoveTowards(burnable.transform.position, targetPos, BurntEffect.dropSpeed * Time.deltaTime);
                        if (burnable.transform.position == targetPos)
                        {
                            burnable.animationFinished = true;
                            continue;
                        }
                    }
                    allAnimationsFinished = false;
                }
                yield return wait;
            }

            burnEffect._fullyBurnt = true;
            burnEffect._dropAnimation = null;
        }

        private Coroutine _growAnimation;
        public Coroutine GrowCoroutine { set => _growAnimation = value; }
        public bool CanStartRegrowAnimation()
        {
            //If burn value is below threshold, make burnables regrow
            return (regrow && burntLevel <= regrowThreshold && _oldBurntLevel > regrowThreshold)
                && (_growAnimation == null && _fullyBurnt);
        }

        public static IEnumerator BurnablesRegrowAnimation(BurntEffect burnEffect)
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            if (burnEffect._dropAnimation != null)
                yield return wait;

            foreach (BurnableObj burnable in burnEffect.burnables)
            {
                burnable.transform.localScale = Vector3.zero;
                burnable.transform.position = burnable.originalWorldPos;
                burnable.animationFinished = false;
            }

            float startTime = Time.time;
            float maxAnimationDur = 10f;
            bool allAnimationsFinished = false;
            while (allAnimationsFinished == false && Time.time < startTime + maxAnimationDur)
            {
                allAnimationsFinished = true;
                //Loop through each burnable, and if it hasn't finished its animation, move it down and mark that at least one animation is not finished.
                foreach (BurnableObj burnable in burnEffect.burnables)
                {
                    if (burnable.animationFinished == true)
                        continue;
                    float growth = (Time.time - (startTime + burnable.animationDelay)) / growTime;
                    if (growth > 0)
                    {
                        burnable.transform.localScale = Vector3.Lerp(burnable.transform.localScale, burnable.originalLocalScale, growth);
                        if (burnable.transform.localScale == burnable.originalLocalScale)
                        {
                            burnable.animationFinished = true;
                            continue;
                        }
                    }
                    allAnimationsFinished = false;
                }
                yield return wait;
            }

            burnEffect._fullyBurnt = false;
            burnEffect._growAnimation = null;
        }


        [System.Serializable]
        public class BurnableObj
        {
            public const float animationDelayRange = 3f;

            public MeshRenderer mesh;
            public Transform transform;
            public Vector3 originalWorldPos;
            public Vector3 originalLocalScale;
            public float animationDelay = 0f;
            public bool animationFinished = false;

            public BurnableObj(MeshRenderer MR)
            {
                mesh = MR;
                transform = MR.transform;
                originalWorldPos = transform.position;
                originalLocalScale = transform.localScale;
                animationDelay = Random.value * animationDelayRange;
            }

            public void RefreshData(float animationRange)
            {
                transform = mesh.transform;
                originalWorldPos = transform.position;
                originalLocalScale = transform.localScale;
                animationDelay = Random.value * animationRange;
            }
        }
    }
}