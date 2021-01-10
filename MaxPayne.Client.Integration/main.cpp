#include "main.h"

using namespace MaxPayne::Client::Application;

MAXPAYNE_CLIENTINTEGRATION_API void on_attach()
{
    MaxPayne::Client::Application::ClientApp::OnAttach();
}

MAXPAYNE_CLIENTINTEGRATION_API void on_frame(const player_info& in, state_from_server& out)
{
    
    //MaxPayne::Messages::State::FrameState^ state = gcnew MaxPayne::Messages::State::FrameState;
    //MaxPayne::Messages::State::FrameState *state = new MaxPayne::Messages::State::FrameState;
    //MaxPayne::Messages::State::FrameState state = MaxPayne::Client::Application::ClientApp::EmptyFrameState();

    #if 0
    state.X = in.x;
    state.Y = in.y;
    state.Z = in.z;
    state.RotationAngle = in.rotation;
    state.VerticalAngle = in.vertical;
    state.Gun = in.gun;
    state.Ammo1 = in.ammo1;
    state.Ammo2 = in.ammo2;
    state.Ammo3 = in.ammo3;
    #endif

    #if 1
    MaxPayne::Client::Application::ClientApp::Frame.X = in.x;
    MaxPayne::Client::Application::ClientApp::Frame.Y = in.y;
    MaxPayne::Client::Application::ClientApp::Frame.Z = in.z;
    MaxPayne::Client::Application::ClientApp::Frame.RotationAngle = in.rotation;
    MaxPayne::Client::Application::ClientApp::Frame.VerticalAngle = in.vertical;
    MaxPayne::Client::Application::ClientApp::Frame.Gun = in.gun;
    MaxPayne::Client::Application::ClientApp::Frame.Ammo1 = in.ammo1;
    MaxPayne::Client::Application::ClientApp::Frame.Ammo2 = in.ammo2;
    MaxPayne::Client::Application::ClientApp::Frame.Ammo3 = in.ammo3;
    #endif
    #if 0
    state->X = in.x;
    state->Y = in.y;
    state->Z = in.z;
    state->RotationAngle = in.rotation;
    state->VerticalAngle = in.vertical;
    state->Gun = in.gun;
    state->Ammo1 = in.ammo1;
    state->Ammo2 = in.ammo2;
    state->Ammo3 = in.ammo3;
    #endif

    MaxPayne::Messages::State::GameState^ game = MaxPayne::Client::Application::ClientApp::OnFrame();


    out.players.clear();
    out.players.reserve(game->Players.Length);
    for (int i = 0; i < game->Players.Length; i++)
    {
        MaxPayne::Messages::State::PlayerState^ player = game->Players[i];

        remote_player native;
        native.id = player->Id;
        native.x = player->X;
        native.y = player->Y;
        native.z = player->Z;
        native.rotation = player->RotationAngle;
        native.vertical = player->VerticalAngle;
        native.gun = player->Gun;

        out.players.push_back(native);
    }
}

MAXPAYNE_CLIENTINTEGRATION_API void on_detach()
{
}
