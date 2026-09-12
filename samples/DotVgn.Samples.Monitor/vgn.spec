Name:           vgn
Version:        %{?pkg_version}%{!?pkg_version:0.0.0}
Release:        0
Summary:        VGN departure monitor
License:        MIT
URL:            https://github.com/rlvelte/dotvgn
Source0:        %{name}-%{version}.tar.gz
BuildArch:      x86_64

%description
Console monitor for VAG/VGN public transport departures,
built with DotVgn (https://github.com/rlvelte/dotvgn).

%prep
%autosetup -n %{name}-%{version}

%install
install -Dm755 vgn %{buildroot}%{_bindir}/vgn

%files
%license LICENSE
%{_bindir}/vgn
