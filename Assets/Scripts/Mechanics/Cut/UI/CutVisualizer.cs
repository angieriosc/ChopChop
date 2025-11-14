// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering.Universal;

// /// <summary>
// /// Visualizador de cortes que muestra un holograma del corte
// /// que se realizará en el objeto asignado a la estación de corte.
// /// </summary>
// public class CutVisualizer : MonoBehaviour
// {
//     [Header("Referencias del Visualizador")]
//     [SerializeField]
//     private DecalProjector topDecalProjector;

//     [Header("Texturas de Corte (en orden)")]
//     [SerializeField]
//     private List<Texture2D> cutTextures;

//     [Header("Efecto de Pulso")]
//     [SerializeField]
//     private float pulseSpeed = 5f;
//     [SerializeField]
//     private float minOpacity = 0.5f;
//     [SerializeField]
//     private float maxOpacity = 1.0f;

//     [Header("Animación de Giro")]
//     [SerializeField]
//     private float spinAngle = 360f;
//     [SerializeField]
//     private float spinDuration = 0.25f;

//     private Coroutine spinAnimation;
//     private Quaternion baseProjectorRotation;

//     private SlicingStation station;
//     private GameObject currentTarget;
//     private int currentSlices = -1;
//     private Material decalMaterialInstance;

//     /// <summary>
//     /// Inicializa las referencias y prepara el visualizador.
//     /// </summary>
//     void Start()
//     {
//         station = GetComponent<SlicingStation>();

//         if (topDecalProjector != null)
//         {
//             decalMaterialInstance = new Material(topDecalProjector.material);
//             topDecalProjector.material = decalMaterialInstance;
//             topDecalProjector.gameObject.SetActive(false);

//             baseProjectorRotation = topDecalProjector.transform.localRotation;
//         }
//     }

//     /// <summary>
//     /// Actualiza el visualizador cada frame para reflejar el estado de la estación de corte.
//     /// </summary>

//     void Update()
//     {
//         if (topDecalProjector == null) return;

//         bool hasObject = (station.objectToCut != null);
//         topDecalProjector.gameObject.SetActive(hasObject);

//         if (station.objectToCut != currentTarget)
//         {
//             currentTarget = station.objectToCut;
//             currentSlices = station.sliceCount;

//             if (currentTarget != null)
//             {
//                 UpdateTexture();
//                 ResetSpin();
//             }
//         }
//         else if (hasObject && station.sliceCount != currentSlices)
//         {
//             currentSlices = station.sliceCount;

//             UpdateTexture();
//             StartSpin();
//         }

//         if (hasObject)
//         {
//             float sineWave = Mathf.Sin(Time.time * pulseSpeed);
//             float normalizedSine = (sineWave + 1f) / 2f;
//             float pulseOpacity = Mathf.Lerp(minOpacity, maxOpacity, normalizedSine);
//             topDecalProjector.fadeFactor = pulseOpacity;
//         }
//     }

//     /// <summary>
//     ///  Actualiza la textura del decal según la cantidad de cortes actuales.
//     /// </summary>
//     private void UpdateTexture()
//     {
//         int textureIndex = currentSlices - 2;

//         if (textureIndex >= 0 && textureIndex < cutTextures.Count)
//         {
//             decalMaterialInstance.SetTexture("Base_Map", cutTextures[textureIndex]);
//         }
//     }

//     /// <summary>
//     /// Resetea la animación de giro del proyector.
//     /// </summary>
//     private void ResetSpin()
//     {
//         if (spinAnimation != null)
//         {
//             StopCoroutine(spinAnimation);
//             spinAnimation = null;
//         }
//         baseProjectorRotation = topDecalProjector.transform.localRotation;
//     }

//     /// <summary>
//     /// Inicia la animación de giro del proyector.
//     /// </summary>
//     private void StartSpin()
//     {
//         if (spinAnimation != null)
//         {
//             StopCoroutine(spinAnimation);
//         }
//         spinAnimation = StartCoroutine(AnimateSpin());
//     }

//     /// <summary>
//     /// Corrutina que anima el giro del proyector.
//     /// </summary>
//     private IEnumerator AnimateSpin()
//     {
//         float timer = 0f;
//         Quaternion startRotation = baseProjectorRotation;

//         while (timer < spinDuration)
//         {
//             float percentage = timer / spinDuration;
//             float currentAngle = Mathf.Lerp(0, spinAngle, percentage);

//             Quaternion currentSpin = Quaternion.Euler(0, currentAngle, 0);
//             topDecalProjector.transform.localRotation = currentSpin * startRotation;

//             timer += Time.deltaTime;
//             yield return null;
//         }

//         Quaternion targetSpin = Quaternion.Euler(0, spinAngle, 0);
//         Quaternion targetRotation = targetSpin * startRotation;

//         topDecalProjector.transform.localRotation = targetRotation;
//         baseProjectorRotation = targetRotation;
//         spinAnimation = null;
//     }

//     /// <summary>
//     /// Oculta el holograma del visualizador.
//     /// </summary>
//     public void HideHologram()
//     {
//         if (topDecalProjector != null)
//         {
//             topDecalProjector.gameObject.SetActive(false);
//         }
//     }
// }