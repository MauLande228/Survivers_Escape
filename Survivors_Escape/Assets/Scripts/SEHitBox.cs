using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEHitBox : MonoBehaviour, IHitDetector
{
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private HurtBoxMask _hurtBoxMask = HurtBoxMask.ENEMY;

    private float _thickness = 0.025f;
    private IHitResponder _hitResponder;
    public INV_ScreenManager inv;

    public IHitResponder HitResponder { get => _hitResponder; set => _hitResponder = value; }

    public void CheckHit()
    {
        Vector3 scaledSize = new Vector3(
            _collider.size.x * transform.lossyScale.x,
            _collider.size.y * transform.lossyScale.y,
            _collider.size.z * transform.lossyScale.z);

        float distance = scaledSize.y - _thickness;
        Vector3 direction = transform.up;
        Vector3 center = transform.TransformPoint(_collider.center);
        Vector3 start = center - direction * (distance / 2);
        Vector3 halfExtents = new Vector3(scaledSize.x, _thickness, scaledSize.z) / 2;
        Quaternion orientation = transform.rotation;

        HitInteraction hitData = null;
        IHurtBox hurtBox = null;
        RaycastHit[] hits = Physics.BoxCastAll(start, halfExtents, direction, orientation, distance, _layerMask);
        foreach (RaycastHit hit in hits)
        {
            hurtBox = hit.collider.GetComponent<IHurtBox>();
            if(hurtBox != null)
            {
                if(hurtBox.Active)
                {
                    if(_hurtBoxMask.HasFlag((HurtBoxMask)hurtBox.Type))
                    {
                        int xdmg = 0;
                        switch (hurtBox.OType)
                        {
                            case 0:
                                xdmg = _hitResponder.LifeDamage;
                                inv.UseSlot();
                                break;
                            case 1:
                                xdmg = _hitResponder.WoodDamage;
                                inv.UseSlot();
                                break;
                            case 2:
                                xdmg = _hitResponder.RockDamage;
                                inv.UseSlot();
                                break;
                            default:
                                break;
                        }
                        hitData = new HitInteraction
                        {
                            Damage = _hitResponder == null ? 0 : xdmg,
                            HitPoint = hit.point == Vector3.zero ? center : hit.point,
                            HitNormal = hit.normal,
                            HurtBox = hurtBox,
                            HitDetector = this
                        };

                        if (hitData.Validate())
                        {
                            hitData.HitDetector.HitResponder?.Response(hitData);
                            hitData.HurtBox.HurtResponder?.Response(hitData);
                        }
                    }
                }
            }
        }
    }
}
