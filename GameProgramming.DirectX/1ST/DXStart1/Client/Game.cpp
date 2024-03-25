#include "pch.h"
#include "Game.h"
#include "Engine.h"

void Game::Init(const WindowInfo& wInfo)
{
	GEngine->Init(wInfo);
}

void Game::Update()
{
	GEngine->Render();
}
