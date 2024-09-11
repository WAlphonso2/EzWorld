
using UnityEngine;

#region PathFollower

namespace CurvedPathGenerator
{
    #region PathFollwer_RequireComponents

    [RequireComponent(typeof(Rigidbody))]
    public class PathFollower : MonoBehaviour
    {
        #region PathFollower_Variables

        public UnityEngine.Events.UnityEvent EndEvent;

        public PathGenerator Generator;

        public float Speed = 100f;

   
        public float DistanceThreshold = 0.2f;


        public float TurningSpeed = 10f;

        public bool IsLoop = false;


        public bool IsMove = true;
        public bool IsEndEventEnable = false;
        private bool checkFlag = false;

        private Rigidbody targetRigidbody;

        private GameObject target;
        private Vector3 nextPath;

        private int pathIndex = 1;

        #endregion PathFollower_Variables

        #region PathFollower_StartMethod

        private void Start()
        {
            targetRigidbody = GetComponent<Rigidbody>();

            if ( Generator != null )
            {
                target = this.gameObject;
                nextPath = Generator.PathList[1];
                this.transform.position = Generator.PathList[0];
            }
        }

        #endregion PathFollower_StartMethod

        #region PathFollower_FixedUpdateMethod

        public void FixedUpdate()
        {

            if ( !IsMove )
            {
                targetRigidbody.velocity = Vector3.zero;
                return;
            }

            if ( Generator == null )
            {
                IsMove = false; checkFlag = false;
                Debug.LogError("no path");
                return;
            }

            if ( !checkFlag )
            {
                checkFlag = true;
                target = this.gameObject;
                nextPath = Generator.PathList[1];
                this.transform.position = Generator.PathList[0];
            }

            Vector3 offset = nextPath - target.transform.position;
            offset.Normalize();
            Quaternion q = Quaternion.LookRotation(offset);
            targetRigidbody.rotation =
                Quaternion.Slerp(targetRigidbody.rotation, q, TurningSpeed * Time.deltaTime);

            offset.Normalize();
            targetRigidbody.velocity = Speed * Time.deltaTime * offset;

            float Distance = Vector3.Distance(nextPath, target.transform.position);

            if ( Distance < DistanceThreshold )
            {

                if ( pathIndex + 1 < Generator.PathList.Count )
                {
                    nextPath = Generator.PathList[++pathIndex];
                }
                else
                {

                    if ( Generator.IsClosed )
                    {

                        if ( IsLoop )
                        {

                            if ( EndEvent != null && IsEndEventEnable )
                            {
                                EndEvent.Invoke();
                            }
                            nextPath = Generator.PathList[0];
                            pathIndex = 0;


                        }
                        else
                        {
                            StopFollow();
                            if ( EndEvent != null && IsEndEventEnable )
                            {
                                EndEvent.Invoke();
                            }
                        }
                    }
                    else
                    {

                        if ( IsLoop )
                        {

                            nextPath = Generator.PathList[1];
                            pathIndex = 1;
                            this.transform.position = Generator.PathList[0];
                            target.transform.LookAt(Generator.PathList[1]);
                            // If repeatEvent isn't null, run method.
                            // repeatEvent null이 아니면, method를 실행
                            if ( EndEvent != null && IsEndEventEnable )
                            {
                                EndEvent.Invoke();
                            }
                        }
                        else
                        {

                            StopFollow();
                            if ( EndEvent != null && IsEndEventEnable )
                            {
                                EndEvent.Invoke();
                            }
                        }
                    }
                }
            }
        }

        #endregion PathFollower_FixedUpdateMethod

        #region PathFollower_GetPassedLengthMethod

        public float GetPassedLength()
        {
            if ( Generator == null ) return -1;

            if ( pathIndex == 1 )
            {
                return ( Generator.PathList[0] - this.transform.position ).magnitude;
            }
            else if ( pathIndex >= Generator.PathList.Count )
            {
                return Generator.GetLength();
            }
            else
            {
                return Generator.PathLengths[pathIndex - 2] + ( Generator.PathList[pathIndex - 1] - this.transform.position ).magnitude;
            }
        }

        #endregion PathFollower_GetPassedLengthMethod

        #region PathFollower_MovementMethod

        public void StopFollow()
        {
            IsMove = false;
        }

        public void StartFollow()
        {
            if ( Generator == null )
            {
                return;
            }
            IsMove = true;
        }

        #endregion PathFollower_MovementMethod
    }

    #endregion PathFollwer_RequireComponents
}

#endregion PathFollower