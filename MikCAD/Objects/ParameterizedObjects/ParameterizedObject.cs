using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MikCAD.Annotations;
using OpenTK.Mathematics;


namespace MikCAD
{
    public abstract class ParameterizedObject : INotifyPropertyChanged
    {
        private bool _selected = false;

        public virtual bool Selected
        {
            get => _selected;
            internal set
            {
                if (_selected && !value)
                {
                    WriteCompositeToModel();
                }
                _selected = value;
            }
        }

        protected virtual void WriteCompositeToModel()
        {
            if (CompositeOperationMatrix != Matrix4.Identity)
            {
                var mat = GetOnlyModelMatrix() * CompositeOperationMatrix;
                CompositeOperationMatrix = Matrix4.Identity;
                var pos = mat.ExtractTranslation();
                var rot = mat.ExtractRotation();
                var scale = mat.ExtractScale();
                rot.ToEulerAngles(out var angles);
                angles[0] = MathHelper.RadiansToDegrees(angles[0]);
                angles[1] = MathHelper.RadiansToDegrees(angles[1]);
                angles[2] = MathHelper.RadiansToDegrees(angles[2]);
                this._rotation = angles;
                this._position = pos;
                this._scale = scale;
                UpdateScaleMatrix();
                UpdateRotationMatrix(Axis.X);
                UpdateRotationMatrix(Axis.Y);
                UpdateRotationMatrix(Axis.Z);
                UpdateTranslationMatrix();
            }
        }

        public virtual bool PositionEnabled => true;
        public virtual bool RotationEnabled => true;
        public virtual bool ScaleEnabled => true;
        
        private string _name = "";
        public virtual String Name
        {
            get => _name;
            set
            {
                if (Scene.CurrentScene.ObjectsController.IsNameTaken(value))
                {
                    return;
                }    
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public float posX
        {
            get => _position.X;
            set
            {
                _position.X = value;
                UpdateTranslationMatrix();
                OnPropertyChanged(nameof(posX));
                OnPositionUpdate();
            }
        }

        public float posY
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                UpdateTranslationMatrix();
                OnPropertyChanged(nameof(posY));
                OnPositionUpdate();
            }
        }

        public float posZ
        {
            get => _position.Z;
            set
            {
                _position.Z = value;
                UpdateTranslationMatrix();
                OnPropertyChanged(nameof(posZ));
                OnPositionUpdate();
            }
        }

        public float rotX
        {
            get => _rotation.X;
            set
            {
                _rotation.X = value;
                UpdateRotationMatrix(Axis.X);
                OnPropertyChanged(nameof(rotX));
            }
        }

        public float rotY
        {
            get => _rotation.Y;
            set
            {
                _rotation.Y = value;
                UpdateRotationMatrix(Axis.Y);
                OnPropertyChanged(nameof(rotY));
            }
        }

        public float rotZ
        {
            get => _rotation.Z;
            set
            {
                _rotation.Z = value;
                UpdateRotationMatrix(Axis.Z);
                OnPropertyChanged(nameof(rotZ));
            }
        }

        public float scaleX
        {
            get => _scale.X;
            set
            {
                _scale.X = value;
                UpdateScaleMatrix();
                OnPropertyChanged(nameof(scaleX));
            }
        }

        public float scaleY
        {
            get => _scale.Y;
            set
            {
                _scale.Y = value;
                UpdateScaleMatrix();
                OnPropertyChanged(nameof(scaleY));
            }
        }

        public float scaleZ
        {
            get => _scale.Z;
            set
            {
                _scale.Z = value;
                UpdateScaleMatrix();
                OnPropertyChanged(nameof(scaleZ));
            }
        }
        
        public abstract uint[] lines { get; }

        internal Vector3 _position = new Vector3();
        internal Vector3 _rotation = new Vector3();
        internal Vector3 _scale = new Vector3(1, 1, 1);
        
        public abstract void UpdateTranslationMatrix();
        public abstract void UpdateRotationMatrix(Axis axis);
        public abstract void UpdateScaleMatrix();
        
        protected ParameterizedObject(string name)
        {
            int count = 1;
            var objname = name;
            while (Scene.CurrentScene.ObjectsController.IsNameTaken(objname))
            {
                objname = name + $"({count++})";
            }
            _name = objname;
            if(this is not Pointer3D)
                _position = Scene.CurrentScene.ObjectsController._pointer._position;
        }

        public virtual void OnPositionUpdate()
        {
            ;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract void GenerateVertices(uint vertexAttributeLocation, uint normalAttributeLocation);
        public abstract Matrix4 GetModelMatrix();
        public abstract Matrix4 GetOnlyModelMatrix();
        
        public Matrix4 CompositeOperationMatrix { get; set; } = Matrix4.Identity;

        public virtual void OnDelete()
        {
            
        }

        public abstract void Rasterize(Rasterizer rasterizer, uint vertexAttributeLocation, uint normalAttributeLocation);
    }
}