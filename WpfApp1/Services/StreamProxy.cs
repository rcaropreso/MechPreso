using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Schema.KRPC;
using WpfApp1.Services;
using WpfApp1.Utils;
using WpfApp1.Models;

namespace WpfApp1
{
    /// <summary>
    /// Class used to define and share streams
    /// </summary>
    public class StreamProxy : IAsyncMessageUpdate
    {
        private Connection m_conn;

        private Stream<double> _ApoapsisAltitudeStream;
        private Stream<double> _PeriapsisAltitudeStream;
        private Stream<double> _SurfaceAltitudeStream;
        private Stream<double> _MeanAltitudeStream;
        private Stream<double> _UTStream;
        private Stream<float> _SRBFuelStream;
        private Stream<double> _RemainingDeltaVStream;
        private Stream<float> _TerminalVelocityStream;
        private Stream<float> _VesselPitchStream;
        private Stream<float> _VesselHeadingStream;
        private Stream<double> _NodeTimeTo;
        private Stream<double> _CurrentSpeedStream;
        private Stream<double> _HorizontalSpeedStream;
        private Stream<double> _VerticalSpeedStream;


        public Stream<double> ApoapsisAltitudeStream {get => _ApoapsisAltitudeStream; }
        public Stream<double> PeriapsisAltitudeStream { get => _PeriapsisAltitudeStream; }
        public Stream<double> SurfaceAltitudeStream { get => _SurfaceAltitudeStream; }
        public Stream<double> MeanAltitudeStream { get => _MeanAltitudeStream; }
        public Stream<double> UTStream { get => _UTStream; }
        public Stream<float> SRBFuelStream { get => _SRBFuelStream; }
        public Stream<double> RemainingDeltaVStream { get => _RemainingDeltaVStream; }
        public Stream<float> TerminalVelocityStream { get => _TerminalVelocityStream; }
        public Stream<float> VesselPitchStream { get => _VesselPitchStream; }
        public Stream<float> VesselHeadingStream { get => _VesselHeadingStream; }
        public Stream<double> NodeTimeTo { get => _NodeTimeTo; }
        public Stream<double> CurrentSpeedStream { get => _CurrentSpeedStream; }
        public Stream<double> HorizontalSpeedStream { get => _HorizontalSpeedStream; }
        public Stream<double> VerticalSpeedStream { get => _VerticalSpeedStream; }
        
        public ReferenceFrame CurrentReferenceFrame  { get; }

        public Vessel CurrentVessel { get => m_conn.SpaceCenter().ActiveVessel; }

        public StreamProxy(Connection conn, ReferenceFrame refFrame)
        {
            m_conn = conn;
            CurrentReferenceFrame = refFrame;
        }

        public void CreateVesselHeadingStream()
        {
            _VesselHeadingStream?.Remove();
            _VesselHeadingStream = null;

            var flight = CurrentVessel.Flight(this.CurrentReferenceFrame);
            _VesselHeadingStream = m_conn.AddStream(() => flight.Heading);
        }

        public void CreateVesselPitchStream()
        {
            _VesselPitchStream?.Remove();
            _VesselPitchStream = null;

            var flight = CurrentVessel.Flight(this.CurrentReferenceFrame);
            _VesselPitchStream = m_conn.AddStream(() => flight.Pitch);
        }

        public void CreateApoapsisAltitudeStream()
        {
            _ApoapsisAltitudeStream?.Remove();
            _ApoapsisAltitudeStream = null;

            _ApoapsisAltitudeStream = m_conn.AddStream(() => CurrentVessel.Orbit.ApoapsisAltitude);
        }

        public void CreateMeanAltitudeStream()
        {
            _MeanAltitudeStream?.Remove();
            _MeanAltitudeStream = null;

            var flight = CurrentVessel.Flight();
            _MeanAltitudeStream = m_conn.AddStream(() => flight.SurfaceAltitude);
        }

        public void CreatePeriapsisAltitudeStream()
        {
            _PeriapsisAltitudeStream?.Remove();
            _PeriapsisAltitudeStream = null;

            _PeriapsisAltitudeStream = m_conn.AddStream(() => CurrentVessel.Orbit.PeriapsisAltitude);
        }

        public void CreateSurfaceAltitudeStream()
        {
            _SurfaceAltitudeStream?.Remove();
            _SurfaceAltitudeStream = null;

            var flight = CurrentVessel.Flight(this.CurrentReferenceFrame);
            _SurfaceAltitudeStream = m_conn.AddStream(() => flight.SurfaceAltitude);
        }

        public void CreateUTStream()
        {
            _UTStream?.Remove();
            _UTStream = null;

            _UTStream = m_conn.AddStream(() => m_conn.SpaceCenter().UT);
        }

        public void CreateSolidFuelStream(int iStage)
        {
            if(iStage == 0)
            {
                return;
            }

            _SRBFuelStream?.Remove();
            _SRBFuelStream = null;

            var stage_srb_resources = CurrentVessel.ResourcesInDecoupleStage(iStage, false);
            _SRBFuelStream = m_conn.AddStream(() => stage_srb_resources.Amount("SolidFuel"));
        }

        public void CreateRemainingDeltaVStream()
        {
            _RemainingDeltaVStream?.Remove();
            _RemainingDeltaVStream = null;

            var nodes = CurrentVessel.Control.Nodes;
            if (nodes.Count != 0)
            {
                var node = nodes[0];
                //var remaining_burn = m_conn.AddStream(() => node.RemainingBurnVector(node.ReferenceFrame) );
                _RemainingDeltaVStream = m_conn.AddStream(() => node.RemainingDeltaV);
            }
        }

        public void CreateTerminalVelocityStream()
        {
            _TerminalVelocityStream?.Remove();
            _TerminalVelocityStream = null;

            var flight = CurrentVessel.Flight(this.CurrentReferenceFrame);
            _TerminalVelocityStream = m_conn.AddStream(() => flight.TerminalVelocity);
        }

        public void CreateNodeTimeTo()
        {
            _NodeTimeTo?.Remove();
            _NodeTimeTo = null;

            var nodes = CurrentVessel.Control.Nodes;
            if (nodes.Count != 0)
            {
                var node = nodes[0];
                //var remaining_burn = m_conn.AddStream(() => node.RemainingBurnVector(node.ReferenceFrame) );
                _NodeTimeTo = m_conn.AddStream(() => node.TimeTo);
            }
        }
        public void CreateCurrentSpeedStream()
        {
            _CurrentSpeedStream?.Remove();
            _CurrentSpeedStream = null;

            var flight = CurrentVessel.Flight(this.CurrentReferenceFrame);
            _CurrentSpeedStream = m_conn.AddStream(() => flight.Speed);
        }

        public void CreateHorizontalSpeedStream()
        {
            _HorizontalSpeedStream?.Remove();
            _HorizontalSpeedStream = null;

            var flight = CurrentVessel.Flight(this.CurrentReferenceFrame);
            _HorizontalSpeedStream = m_conn.AddStream(() => flight.HorizontalSpeed);
        }

        public void CreateVerticalSpeedStream()
        {
            _VerticalSpeedStream?.Remove();
            _VerticalSpeedStream = null;

            var flight = CurrentVessel.Flight(this.CurrentReferenceFrame);
            _VerticalSpeedStream = m_conn.AddStream(() => flight.VerticalSpeed);
        }

        public void RemoveStreams()
        {
            try
            {
                _ApoapsisAltitudeStream?.Remove();
                _PeriapsisAltitudeStream?.Remove();
                _SurfaceAltitudeStream?.Remove();
                _MeanAltitudeStream?.Remove();
                _UTStream?.Remove();
                _SRBFuelStream?.Remove();
                _RemainingDeltaVStream?.Remove();
                _TerminalVelocityStream?.Remove();
                _VesselPitchStream?.Remove();
                _VesselHeadingStream?.Remove();
                _NodeTimeTo?.Remove();
                _CurrentSpeedStream?.Remove();
                _HorizontalSpeedStream?.Remove();
                _VerticalSpeedStream?.Remove();
            }
            catch (System.IO.IOException e)
            {
                //throw;
            }            
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }
    }
}