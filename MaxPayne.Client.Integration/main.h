#pragma once
#include <vector>

#ifdef MAXPAYNE_CLIENTINTEGRATION_EXPORT
#define MAXPAYNE_CLIENTINTEGRATION_API __declspec(dllexport)
#else
#define MAXPAYNE_CLIENTINTEGRATION_API __declspec(dllimport)
#endif


struct MAXPAYNE_CLIENTINTEGRATION_API remote_player
{
    int id;
    float x, y, z;
    float rotation, vertical;
    char gun;
};

struct MAXPAYNE_CLIENTINTEGRATION_API player_info
{
    float x, y, z;
    float rotation, vertical;
    char gun;
    short ammo1, ammo2, ammo3;
};

struct MAXPAYNE_CLIENTINTEGRATION_API state_from_server
{
    std::vector<remote_player> players;
};


MAXPAYNE_CLIENTINTEGRATION_API void on_attach();
MAXPAYNE_CLIENTINTEGRATION_API void on_frame(const player_info &input, state_from_server &output);
MAXPAYNE_CLIENTINTEGRATION_API void on_detach();


