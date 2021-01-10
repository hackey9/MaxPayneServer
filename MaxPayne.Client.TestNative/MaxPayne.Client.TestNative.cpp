#include <iostream>
#include "../MaxPayne.Client.Integration/main.h"

int main()
{
    std::cout << "Hello World!\n";

    on_attach();

    while(true)
    {
        
        try
        {
            const player_info frame{ 1.f, 1.f, 1.f, 20.f, 30.f };
            state_from_server game;

            on_frame(frame, game);
        }
        catch (void*)
        {
        }
    }
    on_detach();
}

