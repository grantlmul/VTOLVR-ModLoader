﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.Collections;
using UnityEngine.SceneManagement;

namespace NetworkedObjects.Vehicles
{
    public class AV42cNetworkedObjectReceiver : MonoBehaviour
    {
        public UnityClient client;
        public Transform worldCenter;
        public NetworkingManager manager;
        public Player player;
        public ushort id;
        public bool isAI;
        private Transform temp;

        //Classes we use to set the information
        private WheelsController wheelsController;
        private AeroController aeroController;
        private TiltController tiltController;
        private AIPilot aiPilot;
        private AutoPilot autoPilot;
        private WheelsController wheelController;
        private void Start()
        {
            wheelsController = GetComponent<WheelsController>();
            aeroController = GetComponent<AeroController>();
            tiltController = GetComponent<TiltController>();

            temp = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            temp.position = new Vector3(0, 0, 0);
            temp.transform.localScale = new Vector3(10, 10, 10);

            aiPilot = GetComponent<AIPilot>();
            autoPilot = GetComponent<AutoPilot>();
            wheelController = GetComponent<WheelsController>();
        }

        public void SetReceiver()
        {
            if (client)
                client.MessageReceived += MessageReceived;
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                ushort tag = message.Tag;
                switch(tag)
                {
                    case (ushort)Tags.AV42c_General:
                        AV42CGeneralReceived(message.GetReader());
                        break;
                }
            }
        }

        private void AV42CGeneralReceived(DarkRiftReader reader)
        {
            while (reader.Position < reader.Length)
            {
                ushort id = reader.ReadUInt16();

                if (this.id == id)
                {
                    float positionX = reader.ReadSingle();
                    float positionY = reader.ReadSingle();
                    float positionZ = reader.ReadSingle();

                    float rotationX = reader.ReadSingle();
                    float rotationY = reader.ReadSingle();
                    float rotationZ = reader.ReadSingle();

                    float speed = reader.ReadSingle();
                    bool landingGear = reader.ReadBoolean();
                    float flaps = reader.ReadSingle();
                    float thrusterAngle = reader.ReadSingle();

                    float pitch = reader.ReadSingle();
                    float yaw = reader.ReadSingle();
                    float roll = reader.ReadSingle();
                    float breaks = reader.ReadSingle();
                    float throttle = reader.ReadSingle();
                    float wheels = reader.ReadSingle();

                    player.SetPosition(positionX, positionY, positionZ);
                    player.SetRotation(rotationX, rotationY, rotationZ);
                    player.speed = speed;
                    player.landingGear = landingGear;
                    player.flaps = flaps;
                    player.thrusterAngle = thrusterAngle;
                    player.pitch = pitch;
                    player.yaw = yaw;
                    player.roll = roll;
                    player.breaks = breaks;
                    player.throttle = throttle;
                    player.wheels = wheels;

                    manager.UpdatePlayerListString();

                    temp.position = worldCenter.position - new Vector3(positionX, positionY, positionZ);
                    temp.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);

                    //transform.position = worldCenter.position - new Vector3(positionX, positionY, positionZ);
                    //transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);

                    UpdateAI();
                }
                else
                    return;
            }
        }

        private void UpdateAI()
        {
            Vector3 input = player.GetPitchYawRoll();
            float breaks = player.breaks;
            bool landingGear = player.landingGear;
            float flaps = player.flaps;
            float thrusterAngle = player.thrusterAngle;
            float throttle = player.throttle;
            float wheels = player.wheels;

            aiPilot.commandState = AIPilot.CommandStates.Override;
            
            if (wheelsController.gearAnimator.GetCurrentState() == (landingGear ? GearAnimator.GearStates.Extended : GearAnimator.GearStates.Retracted))
            {
                wheelsController.SetGear(landingGear);
            }

            if (aeroController.flaps != flaps)
                aeroController.SetFlaps(flaps);
            if (tiltController.currentTilt != thrusterAngle)
                tiltController.SetTiltImmediate(thrusterAngle);
            if (aeroController.input != input)
                aeroController.input = input;


            autoPilot.OverrideSetThrottle(throttle);
            wheelController.SetBrakes(breaks);
            wheelController.SetBrakeLock(-1);
            wheelController.SetWheelSteer(wheels);

            /*
            foreach (ModuleEngine engine in engines)
            {
                engine.SetThrottle(throttle);
            }

                        if (aeroController.brake != breaks)
                aeroController.SetBrakes(breaks);
            */

        }
        void OnGUI()
        {
            if (GUI.Button(new Rect(100,100,100,100), "Cam"))
            {

                GameObject cam = new GameObject("Cam", typeof(Camera));
                cam.transform.position = new Vector3(0,1000,0);
            }
        }
    }


}
