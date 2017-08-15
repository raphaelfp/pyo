# Pyo

Pyo is an application I built with a friend for an assignment in my third year in college. It's a Paint like app built with C#/.NET and Windows Forms that uses the [Myo armband](https://www.myo.com/) instead of a mouse and a keyboard.

One of the main goals of the assignment was to build an app for people with some types of disabilities, people without a hand or part of it. But it also can be used by people without disabilities.

The app uses the [MyoSharp Library](https://www.codeproject.com/Articles/826194/Controlling-a-Myo-Armband-with-Csharp) and it was updated to .NET 4.5.2. The code is in portuguese because it was presented in my college and one of the requirements is that it should be written in portuguese. I live in Brazil =)

![Pyo](https://raw.githubusercontent.com/raphaelfp/pyo/master/Pyo.jpg)

## Myo

The [Myo armband](https://www.myo.com/) is a gesture recognition device worn on the forearm and manufactured by Thalmic Labs. The Myo enables the user to control technology wirelessly using various hand motions. It uses a set of electromyographic (EMG) sensors that sense electrical activity in the forearm muscles, combined with a gyroscope, accelerometer and magnetometer to recognize gestures. The Myo can be used to control video games, presentations, music and visual entertainment. It differs from the Leap Motion device as it is worn rather than a 3D array of cameras that sense motion in the environment.

## Setup

* Clone this repo 
* Install [Code Contracts](https://visualstudiogallery.msdn.microsoft.com/1ec7db13-3363-46c9-851f-1ce455f66970)
* Install the [Myo app](https://www.myo.com/start)
* Run the code (I advise using Visual Studio 2015 or 2017)

## Controls

| Tab option    | Myo Pose        | Description                |
| ------------- | --------------- | -------------------------- |
| Global        | Fingers Spread  | Change the selected option |
| Draw          | Wave in         | Select the next color      |
| Draw          | Wave out        | Select the previous color  |
| Draw          | Fist            | Hold to draw               |
| Rotate 90º    | Fist            | Rotate 90º                 |
| Rotate 180º   | Fist            | Rotate 180º                |
| Shape         | Wave in         | Select next shape          |
| Shape         | Wave out        | Select previous shape      |
| Shape         | Fist            | Draw selected shape        |
| Shape         | Double tap      | Draw a point               |
| Brush size    | Wave in         | Increases the brush size   |
| Brush size    | Wave out        | Decreases the brush size   |

The original code was written in 2015, I'm just migrating all my repos from VSTS to Github, so they are shared publicly.
