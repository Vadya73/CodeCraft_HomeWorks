using UnityEngine;

namespace Modules.Utils
{
    public class SkyboxMover : MonoBehaviour
    {
        public Material skyboxMat;      // можно не задавать — возьмёт RenderSettings.skybox
        public float speed = 0.15f;     // скорость (частота) для качания
        public float maxAngle = 10f;    // амплитуда наклона вверх/вниз
        public bool usePingPong; // true = плавно туда-сюда, false = постоянно в одну сторону

        float x;

        void Start()
        {
            if (!skyboxMat) skyboxMat = RenderSettings.skybox;
            if (skyboxMat) x = skyboxMat.GetFloat("_RotationX");
        }

        void Update()
        {
            if (!skyboxMat) return;

            if (usePingPong)
                x = Mathf.Sin(Time.time * speed) * maxAngle;
            else
                x += speed * Time.deltaTime;

            skyboxMat.SetFloat("_RotationX", x);
        }
    }
}