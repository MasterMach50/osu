﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Collections.Generic;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogBadges : Container
    {
        private const float container_height = 106.5f;
        private const float vertical_padding = 20;
        private const float horizontal_padding = 85;

        public delegate void SelectionHandler(APIChangelogBuild releaseStream, EventArgs args);

        public event SelectionHandler Selected;

        private readonly FillFlowContainer<StreamBadge> badgesContainer;
        private long selectedStreamId = -1;

        public ChangelogBadges()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(32, 24, 35, 255),
                },
                badgesContainer = new FillFlowContainer<StreamBadge>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Vertical = vertical_padding, Horizontal = horizontal_padding },
                },
            };
        }

        public void Populate(List<APIChangelogBuild> latestBuilds)
        {
            foreach (APIChangelogBuild updateStream in latestBuilds)
            {
                var streamBadge = new StreamBadge(updateStream);
                streamBadge.Selected += onBadgeSelected;
                badgesContainer.Add(streamBadge);
            }
        }

        public void SelectNone()
        {
            selectedStreamId = -1;

            if (badgesContainer != null)
            {
                foreach (StreamBadge streamBadge in badgesContainer)
                {
                    if (!IsHovered)
                        streamBadge.Activate();
                    else
                        streamBadge.Deactivate();
                }
            }
        }

        public void SelectUpdateStream(string updateStream)
        {
            foreach (StreamBadge streamBadge in badgesContainer)
            {
                if (streamBadge.LatestBuild.UpdateStream.Name == updateStream)
                {
                    selectedStreamId = streamBadge.LatestBuild.UpdateStream.Id;
                    streamBadge.Activate();
                }
                else
                    streamBadge.Deactivate();
            }
        }

        private void onBadgeSelected(StreamBadge source, EventArgs args)
        {
            selectedStreamId = source.LatestBuild.UpdateStream.Id;
            OnSelected(source);
        }

        protected virtual void OnSelected(StreamBadge source)
        {
            Selected?.Invoke(source.LatestBuild, EventArgs.Empty);
        }

        protected override bool OnHover(HoverEvent e)
        {
            foreach (StreamBadge streamBadge in badgesContainer.Children)
            {
                if (selectedStreamId >= 0)
                {
                    if (selectedStreamId != streamBadge.LatestBuild.UpdateStream.Id)
                        streamBadge.Deactivate();
                    else
                        streamBadge.EnableDim();
                }
                else
                    streamBadge.Deactivate();
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            foreach (StreamBadge streamBadge in badgesContainer.Children)
            {
                if (selectedStreamId < 0)
                    streamBadge.Activate();
                else if (streamBadge.LatestBuild.UpdateStream.Id == selectedStreamId)
                    streamBadge.DisableDim();
            }

            base.OnHoverLost(e);
        }
    }
}
