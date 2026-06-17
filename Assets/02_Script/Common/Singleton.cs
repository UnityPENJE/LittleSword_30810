using UnityEngine;

namespace LittleSword.Common
{
    /// <summary>
    /// 제네릭 싱글턴 베이스 클래스입니다.
    /// T는 MonoBehaviour를 상속해야 하며, 씬에서 기존 인스턴스를 찾거나 없으면
    /// 새 GameObject를 생성하여 인스턴스를 만듭니다.
    /// 생성된 인스턴스는 씬 전환 시 파괴되지 않도록 DontDestroyOnLoad로 설정됩니다.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // 싱글턴 인스턴스(캐시)
        private static T instance;

        /// <summary>
        /// 싱글턴 인스턴스 접근자.
        /// 인스턴스가 아직 생성되지 않은 경우:
        /// 1) 씬에서 T 타입의 첫 번째 컴포넌트를 찾아 할당합니다.
        /// 2) 씬에 존재하지 않으면 새 GameObject를 생성하고
        ///    T 컴포넌트를 추가하여 인스턴스로 사용합니다.
        /// 생성된 오브젝트는 DontDestroyOnLoad로 설정됩니다.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    // 씬에서 T 타입의 컴포넌트를 찾아서 할당
                    instance = FindFirstObjectByType<T>();
                    if (instance == null)
                    {
                        // 씬에 없으면 새 GameObject를 만들고 컴포넌트를 추가
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Awake에서 인스턴스를 초기화하고, 이미 다른 인스턴스가 존재하면 자신을 파괴합니다.
        /// </summary>
        protected void Awake()
        {
            if (instance == null)
            {
                // 현재 오브젝트를 싱글턴 인스턴스로 등록
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                // 중복 인스턴스가 존재하면 현재 오브젝트는 제거
                Destroy(gameObject);
            }
        }
    }
}

























