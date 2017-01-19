Microsoft Cognitive Services サンプルアプリケーション
=====================================================
Overview
##Description
Facebookアカウント内の画像を、Microsoft Cognitive Servicesを利用し、
画像解析を実施します。

## Requirement
以下、2点ご準備ください。
①Azure サブスクリプション内に、以下のAzure サービスを事前に作成下さい。
Web.configにて、作成した各サービスの認証情報を入力してご利用下さい。

・Cognitive Services アカウント
・Azure DocumentDB　
・Azure Storage アカウント
・Web Apps

②Facebookディベロッパー画面にてアプリケーション登録
https://docs.microsoft.com/ja-jp/azure/app-service-mobile/app-service-mobile-how-to-configure-facebook-authentication

## Install
NugetPackageにて、SDKライブラリ等を管理しています。
ソリューションダウンロード時に、プロジェクト単位で復元されますので、
表示される画面の指示に従って進めてください。

## Author
https://github.com/motomichi/cognitive-test